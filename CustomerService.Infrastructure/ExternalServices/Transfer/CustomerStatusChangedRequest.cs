namespace CustomerService.Infrastructure.ExternalServices.Transfer;

public sealed record CustomerStatusChangedRequest(
    Guid CustomerId,
    string OldStatus,
    string NewStatus);
