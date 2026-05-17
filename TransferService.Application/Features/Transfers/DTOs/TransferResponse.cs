using TransferService.Domain.Enums;

namespace TransferService.Application.Features.Transfers.DTOs;

public sealed record TransferResponse(
    Guid Id,
    string TransactionCode,
    TransferStatus Status,
    decimal Amount,
    string Currency,
    decimal TryAmount,
    decimal Fee,
    FraudRiskLevel FraudRiskLevel,
    decimal? ExchangeRate,
    Guid SenderCustomerId,
    Guid ReceiverCustomerId,
    DateTime? ApprovalAvailableAt,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt);
