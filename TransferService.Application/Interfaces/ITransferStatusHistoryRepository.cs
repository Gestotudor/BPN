using TransferService.Domain.Entities;

namespace TransferService.Application.Interfaces;

public interface ITransferStatusHistoryRepository
{
    Task AddAsync(TransferStatusHistory history, CancellationToken cancellationToken = default);
}
