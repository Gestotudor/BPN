using TransferService.Application.Features.Transfers.DTOs;
using TransferService.Domain.Entities;

namespace TransferService.Application.Common.Mappings;

public static class TransferMappings
{
    public static TransferResponse ToResponse(this Transfer transfer)
    {
        return new TransferResponse(
            transfer.Id,
            transfer.TransactionCode,
            transfer.Status,
            transfer.Amount,
            transfer.Currency,
            transfer.TryAmount,
            transfer.Fee,
            transfer.FraudRiskLevel,
            transfer.ExchangeRate,
            transfer.SenderCustomerId,
            transfer.ReceiverCustomerId,
            transfer.ApprovalAvailableAt,
            transfer.CreatedAt,
            transfer.CompletedAt,
            transfer.CancelledAt);
    }
}
