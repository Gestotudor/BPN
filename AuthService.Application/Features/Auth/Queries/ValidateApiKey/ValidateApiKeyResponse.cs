namespace AuthService.Application.Features.Auth.Queries.ValidateApiKey;

public sealed record ValidateApiKeyResponse(
    bool IsValid,
    bool IsAuthorized,
    Guid? ClientId,
    string? ClientName,
    IReadOnlyCollection<string> Scopes);
