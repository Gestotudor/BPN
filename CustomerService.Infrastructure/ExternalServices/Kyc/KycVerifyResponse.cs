namespace CustomerService.Infrastructure.ExternalServices.Kyc;

public sealed record KycVerifyResponse(
    bool Success,
    string? Message,
    string? ReferenceId);
