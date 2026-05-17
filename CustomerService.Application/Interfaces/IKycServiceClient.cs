namespace CustomerService.Application.Interfaces;

public interface IKycServiceClient
{
    Task<KycVerificationResult> VerifyAsync(
        Guid userId,
        string nationalIdNumber,
        string name,
        string surname,
        DateTime dateOfBirth,
        CancellationToken cancellationToken = default);
}

public sealed record KycVerificationResult(
    bool IsSuccess,
    string? ErrorMessage,
    string? ExternalReference,
    bool IsServiceUnavailable);
