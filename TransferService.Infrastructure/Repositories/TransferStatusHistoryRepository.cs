using TransferService.Application.Interfaces;
using TransferService.Domain.Entities;
using TransferService.Infrastructure.Persistence;

namespace TransferService.Infrastructure.Repositories;

public sealed class TransferStatusHistoryRepository : ITransferStatusHistoryRepository
{
    private readonly TransferDbContext _dbContext;

    public TransferStatusHistoryRepository(TransferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(TransferStatusHistory history, CancellationToken cancellationToken = default)
    {
        _dbContext.TransferStatusHistories.Add(history);
        return Task.CompletedTask;
    }
}
