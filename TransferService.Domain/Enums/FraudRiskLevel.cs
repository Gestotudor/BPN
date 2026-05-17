namespace TransferService.Domain.Enums;

/// <summary>
/// Fraud risk decision returned by the fraud detection service.
/// </summary>
public enum FraudRiskLevel
{
    /// <summary>
    /// Transfer can proceed.
    /// </summary>
    Low = 1,

    /// <summary>
    /// Transfer must be rejected.
    /// </summary>
    High = 2
}
