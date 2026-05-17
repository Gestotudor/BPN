using MediatR;
using Microsoft.Extensions.Logging;
using TransferService.Application.Common.Mappings;
using TransferService.Application.Common.Results;
using TransferService.Application.Features.Transfers.DTOs;
using TransferService.Application.Interfaces;
using TransferService.Domain.Entities;
using TransferService.Domain.Enums;

namespace TransferService.Application.Features.Transfers.Commands.CancelTransfer;

public sealed class CancelTransferCommandHandler : IRequestHandler<CancelTransferCommand, Result<TransferResponse>>
{
    private readonly ITransferRepository _transferRepository;
    private readonly ITransferStatusHistoryRepository _transferStatusHistoryRepository;
    private readonly ITransferFeeRefundRepository _transferFeeRefundRepository;
    private readonly ILogger<CancelTransferCommandHandler> _logger;

    public CancelTransferCommandHandler(
        ITransferRepository transferRepository,
        ITransferStatusHistoryRepository transferStatusHistoryRepository,
        ITransferFeeRefundRepository transferFeeRefundRepository,
        ILogger<CancelTransferCommandHandler> logger)
    {
        _transferRepository = transferRepository;
        _transferStatusHistoryRepository = transferStatusHistoryRepository;
        _transferFeeRefundRepository = transferFeeRefundRepository;
        _logger = logger;
    }

    public async Task<Result<TransferResponse>> Handle(CancelTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _transferRepository.GetByIdAsync(request.TransferId, cancellationToken);
        if (transfer is null)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.NotFound, "Transfer not found.");
        }

        if (transfer.Status != TransferStatus.Pending)
        {
            return Result<TransferResponse>.Failure(ResultErrorType.Validation, "Only pending transfers can be cancelled.");
        }

        var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Customer request" : request.Reason.Trim();
        var oldStatus = transfer.Status;
        transfer.Status = TransferStatus.Cancelled;
        transfer.CancelledAt = DateTime.UtcNow;

        await _transferStatusHistoryRepository.AddAsync(
            new TransferStatusHistory
            {
                Id = Guid.NewGuid(),
                TransferId = transfer.Id,
                OldStatus = oldStatus,
                NewStatus = transfer.Status,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            },
            cancellationToken);

        await _transferFeeRefundRepository.AddAsync(
            new TransferFeeRefund
            {
                Id = Guid.NewGuid(),
                TransferId = transfer.Id,
                RefundedFeeAmount = transfer.Fee,
                RefundedAt = DateTime.UtcNow,
                Reason = reason
            },
            cancellationToken);

        await _transferRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Transfer {TransferId} cancelled and fee refunded", transfer.Id);
        return Result<TransferResponse>.Success(transfer.ToResponse());
    }
}
