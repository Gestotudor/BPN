using MediatR;
using Microsoft.Extensions.Logging;
using TransferService.Application.Common.Results;
using TransferService.Application.Interfaces;
using TransferService.Domain.Entities;
using TransferService.Domain.Enums;

namespace TransferService.Application.Features.Transfers.Commands.HandleCustomerStatusChanged;

public sealed class HandleCustomerStatusChangedCommandHandler : IRequestHandler<HandleCustomerStatusChangedCommand, Result<int>>
{
    private readonly ITransferRepository _transferRepository;
    private readonly ITransferStatusHistoryRepository _transferStatusHistoryRepository;
    private readonly ITransferFeeRefundRepository _transferFeeRefundRepository;
    private readonly ILogger<HandleCustomerStatusChangedCommandHandler> _logger;

    public HandleCustomerStatusChangedCommandHandler(
        ITransferRepository transferRepository,
        ITransferStatusHistoryRepository transferStatusHistoryRepository,
        ITransferFeeRefundRepository transferFeeRefundRepository,
        ILogger<HandleCustomerStatusChangedCommandHandler> logger)
    {
        _transferRepository = transferRepository;
        _transferStatusHistoryRepository = transferStatusHistoryRepository;
        _transferFeeRefundRepository = transferFeeRefundRepository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(HandleCustomerStatusChangedCommand request, CancellationToken cancellationToken)
    {
        if (!string.Equals(request.NewStatus, "Blocked", StringComparison.OrdinalIgnoreCase))
        {
            return Result<int>.Success(0);
        }

        var pendingTransfers = await _transferRepository.GetPendingTransfersByCustomerIdAsync(request.CustomerId, cancellationToken);
        foreach (var transfer in pendingTransfers)
        {
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
                    Reason = "Transfer cancelled because customer is blocked.",
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
                    Reason = "Blocked customer pending transfer cancellation."
                },
                cancellationToken);
        }

        await _transferRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Cancelled {Count} pending transfers for blocked customer {CustomerId}", pendingTransfers.Count, request.CustomerId);
        return Result<int>.Success(pendingTransfers.Count);
    }
}
