namespace AuthService.Application.Features.ApiKeys.Queries.GetApiKeys;

/// <summary>
/// Metadata about an API key without exposing the plaintext key.
/// </summary>
/// <param name="Id">Unique identifier of the API key record.</param>
/// <param name="ClientId">Unique identifier of the owning client.</param>
/// <param name="ClientName">Display name of the owning client.</param>
/// <param name="KeyPrefix">Non-sensitive prefix shown for identification in operational tooling.</param>
/// <param name="IsActive">Indicates whether the API key can still authenticate requests.</param>
/// <param name="CreatedAt">UTC timestamp when the key was created.</param>
/// <param name="ExpiresAt">Optional UTC expiration date.</param>
/// <param name="RevokedAt">UTC timestamp when the key was revoked, if revoked.</param>
/// <param name="LastUsedAt">UTC timestamp of the most recent successful use, if any.</param>
/// <param name="Scopes">Scopes currently assigned to the key.</param>
public sealed record GetApiKeysResponse(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string KeyPrefix,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    DateTime? RevokedAt,
    DateTime? LastUsedAt,
    IReadOnlyCollection<string> Scopes);
