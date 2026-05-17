namespace TransferService.Application.Interfaces;

public interface IAuthServiceClient
{
    Task<AuthValidationResult> ValidateAsync(
        string apiKey,
        IReadOnlyCollection<string> requiredScopes,
        CancellationToken cancellationToken = default);
}

public sealed record AuthValidationResult(
    bool IsValid,
    bool IsAuthorized,
    Guid? ClientId,
    string? ClientName,
    IReadOnlyCollection<string> Scopes);
