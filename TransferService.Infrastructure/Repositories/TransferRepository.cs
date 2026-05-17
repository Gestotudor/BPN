using Microsoft.EntityFrameworkCore;
using TransferService.Application.Interfaces;
using TransferService.Domain.Entities;
using TransferService.Domain.Enums;
using TransferService.Infrastructure.Persistence;

namespace TransferService.Infrastructure.Repositories;

public sealed class TransferRepository : ITransferRepository
{
    private readonly TransferDbContext _dbContext;

    public TransferRepository(TransferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Transfer transfer, CancellationToken cancellationToken = default)
    {
        _dbContext.Transfers.Add(transfer);
        return Task.CompletedTask;
    }

    public Task<Transfer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Transfers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Transfer?> GetByTransactionCodeAsync(string transactionCode, CancellationToken cancellationToken = default)
    {
        return _dbContext.Transfers.FirstOrDefaultAsync(x => x.TransactionCode == transactionCode, cancellationToken);
    }

    public Task<bool> ExistsByTransactionCodeAsync(string transactionCode, CancellationToken cancellationToken = default)
    {
        return _dbContext.Transfers.AnyAsync(x => x.TransactionCode == transactionCode, cancellationToken);
    }

    public Task<decimal> GetDailyTotalAmountAsync(
        Guid senderCustomerId,
        DateTime startDateUtc,
        DateTime endDateUtc,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Transfers
            .Where(x =>
                x.SenderCustomerId == senderCustomerId &&
                x.CreatedAt >= startDateUtc &&
                x.CreatedAt < endDateUtc &&
                x.Status != TransferStatus.Cancelled &&
                x.Status != TransferStatus.Failed)
            .SumAsync(x => x.TryAmount, cancellationToken);
    }

    public Task<List<Transfer>> GetPendingTransfersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Transfers
            .Where(x =>
                x.Status == TransferStatus.Pending &&
                (x.SenderCustomerId == customerId || x.ReceiverCustomerId == customerId))
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
