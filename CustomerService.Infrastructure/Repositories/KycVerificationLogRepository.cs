using CustomerService.Application.Interfaces;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;

namespace CustomerService.Infrastructure.Repositories;

public sealed class KycVerificationLogRepository : IKycVerificationLogRepository
{
    private readonly CustomerDbContext _dbContext;

    public KycVerificationLogRepository(CustomerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(KycVerificationLog log, CancellationToken cancellationToken = default)
    {
        await _dbContext.KycVerificationLogs.AddAsync(log, cancellationToken);
    }
}
