namespace TransferService.Application.Features.Transfers.DTOs;

/// <summary>
/// Request payload used to complete a pending transfer.
/// </summary>
/// <param name="TransactionCode">MoneyBee transaction code provided to the receiver.</param>
/// <param name="ReceiverCustomerId">Identifier of the customer receiving the funds.</param>
public sealed record ReceiveTransferRequest(
    string TransactionCode,
    Guid ReceiverCustomerId);
