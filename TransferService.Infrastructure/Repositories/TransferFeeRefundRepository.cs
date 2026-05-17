using TransferService.Application.Interfaces;
using TransferService.Domain.Entities;
using TransferService.Infrastructure.Persistence;

namespace TransferService.Infrastructure.Repositories;

public sealed class TransferFeeRefundRepository : ITransferFeeRefundRepository
{
    private readonly TransferDbContext _dbContext;

    public TransferFeeRefundRepository(TransferDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(TransferFeeRefund refund, CancellationToken cancellationToken = default)
    {
        _dbContext.TransferFeeRefunds.Add(refund);
        return Task.CompletedTask;
    }
}
