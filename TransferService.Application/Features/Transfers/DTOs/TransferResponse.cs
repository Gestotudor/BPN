using TransferService.Domain.Enums;

namespace TransferService.Application.Features.Transfers.DTOs;

/// <summary>
/// Transfer response returned by transfer management endpoints.
/// </summary>
/// <param name="Id">Unique identifier of the transfer.</param>
/// <param name="TransactionCode">Human-readable transaction code used to receive the transfer.</param>
/// <param name="Status">Current status of the transfer.</param>
/// <param name="Amount">Original transfer amount in the requested currency.</param>
/// <param name="Currency">ISO currency code of the original amount.</param>
/// <param name="TryAmount">Equivalent amount in Turkish lira after exchange-rate processing.</param>
/// <param name="Fee">Fee charged for the transfer.</param>
/// <param name="FraudRiskLevel">Fraud screening result for the transfer.</param>
/// <param name="ExchangeRate">Applied exchange rate when the original currency is not TRY.</param>
/// <param name="SenderCustomerId">Identifier of the validated sender customer.</param>
/// <param name="ReceiverCustomerId">Identifier of the validated receiver customer.</param>
/// <param name="ApprovalAvailableAt">UTC timestamp when the transfer becomes receivable after the waiting period.</param>
/// <param name="CreatedAt">UTC timestamp when the transfer was created.</param>
/// <param name="CompletedAt">UTC timestamp when the transfer was completed.</param>
/// <param name="CancelledAt">UTC timestamp when the transfer was cancelled.</param>
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
