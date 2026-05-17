namespace TransferService.Application.Features.Transfers.DTOs;

public sealed record CustomerStatusChangedRequest(
    Guid CustomerId,
    string OldStatus,
    string NewStatus);
