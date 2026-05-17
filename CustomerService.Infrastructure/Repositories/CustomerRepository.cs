using CustomerService.Application.Interfaces;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _dbContext;

    public CustomerRepository(CustomerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(customer, cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Customer?> GetByNationalIdNumberAsync(string nationalIdNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers.FirstOrDefaultAsync(x => x.NationalIdNumber == nationalIdNumber, cancellationToken);
    }

    public Task<bool> ExistsByNationalIdNumberAsync(string nationalIdNumber, CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers.AnyAsync(x => x.NationalIdNumber == nationalIdNumber, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
