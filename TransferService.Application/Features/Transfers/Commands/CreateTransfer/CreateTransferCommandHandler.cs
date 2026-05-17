using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TransferService.Application.Common.Mappings;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;
using TransferService.Application.Interfaces;
using TransferService.Domain.Entities;
using TransferService.Domain.Enums;

namespace TransferService.Application.Features.Transfers.Commands.CreateTransfer;

public sealed class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, Result<TransferResponse>>
{
    private const decimal DailyTransferLimitTry = 10000m;

    private readonly ITransferRepository _transferRepository;
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly ITransferStatusHistoryRepository _transferStatusHistoryRepository;
    private readonly ICustomerServiceClient _customerServiceClient;
    private readonly IExchangeRateClient _exchangeRateClient;
    private readonly IFraudDetectionClient _fraudDetectionClient;
    private readonly ITransferFeeCalculator _transferFeeCalculator;
    private readonly ITransactionCodeGenerator _transactionCodeGenerator;
    private readonly ILogger<CreateTransferCommandHandler> _logger;

    public CreateTransferCommandHandler(
        ITransferRepository transferRepository,
        IIdempotencyRepository idempotencyRepository,
        ITransferStatusHistoryRepository transferStatusHistoryRepository,
        ICustomerServiceClient customerServiceClient,
        IExchangeRateClient exchangeRateClient,
        IFraudDetectionClient fraudDetectionClient,
        ITransferFeeCalculator transferFeeCalculator,
        ITransactionCodeGenerator transactionCodeGenerator,
        ILogger<CreateTransferCommandHandler> logger)
    {
        _transferRepository = transferRepository;
        _idempotencyRepository = idempotencyRepository;
        _transferStatusHistoryRepository = transferStatusHistoryRepository;
        _customerServiceClient = customerServiceClient;
        _exchangeRateClient = exchangeRateClient;
        _fraudDetectionClient = fraudDetectionClient;
        _transferFeeCalculator = transferFeeCalculator;
        _transactionCodeGenerator = transactionCodeGenerator;
        _logger = logger;
    }

    public async Task<Result<TransferResponse>> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        var requestHash = ComputeRequestHash(request);
        var existingRecord = await _idempotencyRepository.GetByKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existingRecord is not null)
        {
            if (!string.Equals(existingRecord.RequestHash, requestHash, StringComparison.Ordinal))
            {
                return Result<TransferResponse>.Failure(ResultErrorType.Conflict, "Idempotency key already used with a different request.");
            }

            var existingResponse = JsonSerializer.Deserialize<TransferResponse>(existingRecord.ResponseJson);
            if (existingResponse is null)
            {
                return Result<TransferResponse>.Failure(ResultErrorType.Conflict, "Idempotent response could not be restored.");
            }

            _logger.LogInformation("Idempotency hit for key {IdempotencyKey}", request.IdempotencyKey);
            return Result<TransferResponse>.Success(existingResponse);
        }

        var senderValidation = await _customerServiceClient.EnsureValidatedAsync(request.SenderCustomer, cancellationToken);
        if (!senderValidation.IsSuccess)
        {
            return senderValidation.IsUnauthorized
                ? Result<TransferResponse>.Failure(ResultErrorType.Forbidden, "Transfer API key is not authorized to manage sender customer.")
                : Result<TransferResponse>.Failure(
                    senderValidation.ErrorType ?? ResultErrorType.ServiceUnavailable,
                    senderValidation.ErrorMessage ?? "Customer request failed for sender customer.");
        }

        if (!senderValidation.IsValid)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.Validation, "Sender customer is invalid.");
        }

        var receiverValidation = await _customerServiceClient.EnsureValidatedAsync(request.ReceiverCustomer, cancellationToken);
        if (!receiverValidation.IsSuccess)
        {
            return receiverValidation.IsUnauthorized
                ? Result<TransferResponse>.Failure(ResultErrorType.Forbidden, "Transfer API key is not authorized to manage receiver customer.")
                : Result<TransferResponse>.Failure(
                    receiverValidation.ErrorType ?? ResultErrorType.ServiceUnavailable,
                    receiverValidation.ErrorMessage ?? "Customer request failed for receiver customer.");
        }

        if (!receiverValidation.IsValid)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.Validation, "Receiver customer is invalid.");
        }

        if (senderValidation.CustomerId == receiverValidation.CustomerId)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.Validation, "Sender and receiver cannot be same.");
        }

        var senderCustomerId = senderValidation.CustomerId!.Value;
        var receiverCustomerId = receiverValidation.CustomerId!.Value;

        var currency = request.Currency.Trim().ToUpperInvariant();
        var exchangeRate = currency == "TRY"
            ? (decimal?)null
            : (await _exchangeRateClient.GetRateAsync(currency, "TRY", cancellationToken)).Rate;

        var tryAmount = exchangeRate.HasValue
            ? Math.Round(request.Amount * exchangeRate.Value, 2, MidpointRounding.AwayFromZero)
            : Math.Round(request.Amount, 2, MidpointRounding.AwayFromZero);

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var dailyTotal = await _transferRepository.GetDailyTotalAmountAsync(senderCustomerId, today, tomorrow, cancellationToken);
        if (dailyTotal + tryAmount > DailyTransferLimitTry)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.Validation, "Daily transfer limit exceeded.");
        }

        var fee = _transferFeeCalculator.Calculate(tryAmount);
        var transactionCode = await _transactionCodeGenerator.GenerateAsync(cancellationToken);

        var fraudResult = await _fraudDetectionClient.CheckAsync(
            transactionCode,
            senderCustomerId,
            receiverCustomerId,
            tryAmount,
            "TRY",
            cancellationToken);

        _logger.LogInformation("Fraud check completed for {TransactionCode} with risk {RiskLevel}", transactionCode, fraudResult.RiskLevel);

        var transfer = new Transfer
        {
            Id = Guid.NewGuid(),
            SenderCustomerId = senderCustomerId,
            ReceiverCustomerId = receiverCustomerId,
            Amount = request.Amount,
            Currency = currency,
            TryAmount = tryAmount,
            ExchangeRate = exchangeRate,
            Fee = fee,
            TransactionCode = transactionCode,
            Status = fraudResult.RiskLevel == FraudRiskLevel.High || fraudResult.ShouldBlock
                ? TransferStatus.Failed
                : TransferStatus.Pending,
            FraudRiskLevel = fraudResult.RiskLevel,
            ApprovalAvailableAt = tryAmount > 1000m ? DateTime.UtcNow.AddMinutes(5) : null,
            CreatedAt = DateTime.UtcNow
        };

        await _transferRepository.AddAsync(transfer, cancellationToken);
        await _transferStatusHistoryRepository.AddAsync(
            new TransferStatusHistory
            {
                Id = Guid.NewGuid(),
                TransferId = transfer.Id,
                OldStatus = null,
                NewStatus = transfer.Status,
                Reason = transfer.Status == TransferStatus.Failed
                    ? "Transfer rejected by fraud detection service."
                    : "Transfer created.",
                ChangedAt = DateTime.UtcNow
            },
            cancellationToken);

        var response = transfer.ToResponse();
        await _idempotencyRepository.AddAsync(
            new IdempotencyRecord
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = request.IdempotencyKey,
                RequestHash = requestHash,
                ResponseJson = JsonSerializer.Serialize(response),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            },
            cancellationToken);

        await _transferRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Transfer created with transaction code {TransactionCode}", transactionCode);
        return Result<TransferResponse>.Success(response);
    }

    private static string ComputeRequestHash(CreateTransferCommand request)
    {
        var serialized = JsonSerializer.Serialize(new
        {
            SenderCustomer = NormalizeCustomer(request.SenderCustomer),
            ReceiverCustomer = NormalizeCustomer(request.ReceiverCustomer),
            request.Amount,
            Currency = request.Currency.Trim().ToUpperInvariant()
        });

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(serialized)));
    }

    private static object NormalizeCustomer(TransferCustomerRequest customer) => new
    {
        Name = customer.Name.Trim(),
        Surname = customer.Surname.Trim(),
        NationalIdNumber = customer.NationalIdNumber.Trim(),
        TaxNumber = string.IsNullOrWhiteSpace(customer.TaxNumber) ? null : customer.TaxNumber.Trim(),
        PhoneNumber = customer.PhoneNumber.Trim(),
        customer.DateOfBirth,
        customer.Type
    };
}
