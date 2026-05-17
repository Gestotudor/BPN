using TransferService.Domain.Enums;

namespace TransferService.Application.Interfaces;

public interface IFraudDetectionClient
{
    Task<FraudCheckResult> CheckAsync(
        string transactionCode,
        Guid senderCustomerId,
        Guid receiverCustomerId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);
}

public sealed record FraudCheckResult(
    FraudRiskLevel RiskLevel,
    decimal? RiskScore,
    bool ShouldBlock,
    IReadOnlyCollection<string> RiskFactors,
    IReadOnlyCollection<string> Recommendations,
    IReadOnlyCollection<string> RequiredActions,
    decimal? ProcessingTime);
