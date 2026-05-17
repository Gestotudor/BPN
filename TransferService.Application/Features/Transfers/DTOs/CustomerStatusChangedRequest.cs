namespace TransferService.Application.Features.Transfers.DTOs;

/// <summary>
/// Internal event payload sent by the Customer service when a customer status changes.
/// </summary>
/// <param name="CustomerId">Identifier of the customer whose status changed.</param>
/// <param name="OldStatus">Previous customer status value.</param>
/// <param name="NewStatus">New customer status value.</param>
public sealed record CustomerStatusChangedRequest(
    Guid CustomerId,
    string OldStatus,
    string NewStatus);
