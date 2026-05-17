using CustomerService.Domain.Entities;

namespace CustomerService.Application.Interfaces;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Customer?> GetByNationalIdNumberAsync(string nationalIdNumber, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNationalIdNumberAsync(string nationalIdNumber, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
