namespace TransferService.Application.Features.Transfers.DTOs;

/// <summary>
/// Request payload used to cancel a pending transfer.
/// </summary>
/// <param name="Reason">Optional reason recorded for the cancellation and fee refund.</param>
public sealed record CancelTransferRequest(string? Reason);
