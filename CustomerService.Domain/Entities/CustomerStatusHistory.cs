using CustomerService.Domain.Enums;

namespace CustomerService.Domain.Entities;

public sealed class CustomerStatusHistory
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;

    public CustomerStatus OldStatus { get; set; }

    public CustomerStatus NewStatus { get; set; }

    public string? Reason { get; set; }

    public DateTime ChangedAt { get; set; }
}
