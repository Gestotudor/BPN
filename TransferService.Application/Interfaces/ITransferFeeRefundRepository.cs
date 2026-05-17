using TransferService.Domain.Entities;

namespace TransferService.Application.Interfaces;

public interface ITransferFeeRefundRepository
{
    Task AddAsync(TransferFeeRefund refund, CancellationToken cancellationToken = default);
}
