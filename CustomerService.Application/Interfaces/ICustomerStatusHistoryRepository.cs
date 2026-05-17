using CustomerService.Domain.Entities;

namespace CustomerService.Application.Interfaces;

public interface ICustomerStatusHistoryRepository
{
    Task AddAsync(CustomerStatusHistory history, CancellationToken cancellationToken = default);
}
