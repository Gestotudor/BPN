using Microsoft.EntityFrameworkCore;
using TransferService.Application.Interfaces;
using TransferService.Domain.Entities;
using TransferService.Infrastructure.Persistence;

namespace TransferService.Infrastructure.Repositories;

public sealed class IdempotencyRepository : IIdempotencyRepository
{
    private readonly TransferDbContext _dbContext;

    public IdempotencyRepository(TransferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IdempotencyRecord?> GetByKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        return _dbContext.IdempotencyRecords.FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public Task AddAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        _dbContext.IdempotencyRecords.Add(record);
        return Task.CompletedTask;
    }
}
