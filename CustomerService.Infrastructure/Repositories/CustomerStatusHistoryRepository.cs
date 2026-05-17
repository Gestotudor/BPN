using CustomerService.Application.Interfaces;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;

namespace CustomerService.Infrastructure.Repositories;

public sealed class CustomerStatusHistoryRepository : ICustomerStatusHistoryRepository
{
    private readonly CustomerDbContext _dbContext;

    public CustomerStatusHistoryRepository(CustomerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(CustomerStatusHistory history, CancellationToken cancellationToken = default)
    {
        await _dbContext.CustomerStatusHistories.AddAsync(history, cancellationToken);
    }
}
