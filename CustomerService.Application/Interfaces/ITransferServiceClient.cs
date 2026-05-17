using CustomerService.Domain.Enums;

namespace CustomerService.Application.Interfaces;

public interface ITransferServiceClient
{
    Task NotifyCustomerStatusChangedAsync(
        Guid customerId,
        CustomerStatus oldStatus,
        CustomerStatus newStatus,
        CancellationToken cancellationToken = default);
}
