using MediatR;
using Microsoft.Extensions.Logging;
using TransferService.Application.Common.Mappings;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;
using TransferService.Application.Interfaces;
using TransferService.Domain.Entities;
using TransferService.Domain.Enums;

namespace TransferService.Application.Features.Transfers.Commands.ReceiveTransfer;

public sealed class ReceiveTransferCommandHandler : IRequestHandler<ReceiveTransferCommand, Result<TransferResponse>>
{
    private readonly ITransferRepository _transferRepository;
    private readonly ITransferStatusHistoryRepository _transferStatusHistoryRepository;
    private readonly ICustomerServiceClient _customerServiceClient;
    private readonly ILogger<ReceiveTransferCommandHandler> _logger;

    public ReceiveTransferCommandHandler(
        ITransferRepository transferRepository,
        ITransferStatusHistoryRepository transferStatusHistoryRepository,
        ICustomerServiceClient customerServiceClient,
        ILogger<ReceiveTransferCommandHandler> logger)
    {
        _transferRepository = transferRepository;
        _transferStatusHistoryRepository = transferStatusHistoryRepository;
        _customerServiceClient = customerServiceClient;
        _logger = logger;
    }

    public async Task<Result<TransferResponse>> Handle(ReceiveTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _transferRepository.GetByTransactionCodeAsync(request.TransactionCode, cancellationToken);
        if (transfer is null)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.NotFound, "Transfer not found.");
        }

        if (transfer.Status != TransferStatus.Pending)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.Validation, "Only pending transfers can be received.");
        }

        var receiverValidation = await _customerServiceClient.ValidateAsync(request.ReceiverCustomerId, cancellationToken);
        if (!receiverValidation.IsSuccess)
        {
            return receiverValidation.IsUnauthorized
                ? Result<TransferResponse>.Failure(ResultErrorType.Forbidden, "Transfer API key is not authorized to validate receiver customer.")
                : Result<TransferResponse>.Failure(
                    receiverValidation.ErrorType ?? ResultErrorType.ServiceUnavailable,
                    receiverValidation.ErrorMessage ?? "Customer validation request failed for receiver customer.");
        }

        if (!receiverValidation.IsValid)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.Validation, "Receiver customer is invalid.");
        }

        if (transfer.ReceiverCustomerId != request.ReceiverCustomerId)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.Forbidden, "Receiver does not match the transfer.");
        }

        if (transfer.ApprovalAvailableAt.HasValue && DateTime.UtcNow < transfer.ApprovalAvailableAt.Value)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.Validation, "Transfer is still in waiting period.");
        }

        var oldStatus = transfer.Status;
        transfer.Status = TransferStatus.Completed;
        transfer.CompletedAt = DateTime.UtcNow;

        await _transferStatusHistoryRepository.AddAsync(
            new TransferStatusHistory
            {
                Id = Guid.NewGuid(),
                TransferId = transfer.Id,
                OldStatus = oldStatus,
                NewStatus = transfer.Status,
                Reason = "Transfer received.",
                ChangedAt = DateTime.UtcNow
            },
            cancellationToken);

        await _transferRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Transfer {TransactionCode} completed", transfer.TransactionCode);
        return Result<TransferResponse>.Success(transfer.ToResponse());
    }
}
