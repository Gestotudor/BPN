namespace AuthService.Application.Features.Auth.Queries.ValidateApiKey;

/// <summary>
/// Result of validating an API key and checking authorization scopes.
/// </summary>
/// <param name="IsValid">Indicates whether the API key exists and is active.</param>
/// <param name="IsAuthorized">Indicates whether the key satisfies the requested scopes.</param>
/// <param name="ClientId">Identifier of the authenticated client when validation succeeds.</param>
/// <param name="ClientName">Display name of the authenticated client when validation succeeds.</param>
/// <param name="Scopes">Scopes assigned to the API key.</param>
public sealed record ValidateApiKeyResponse(
    bool IsValid,
    bool IsAuthorized,
    Guid? ClientId,
    string? ClientName,
    IReadOnlyCollection<string> Scopes);
