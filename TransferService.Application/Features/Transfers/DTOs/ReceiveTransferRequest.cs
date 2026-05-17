namespace TransferService.Application.Features.Transfers.DTOs;

public sealed record ReceiveTransferRequest(
    string TransactionCode,
    Guid ReceiverCustomerId);
