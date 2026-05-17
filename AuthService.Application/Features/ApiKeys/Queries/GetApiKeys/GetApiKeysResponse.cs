namespace AuthService.Application.Features.ApiKeys.Queries.GetApiKeys;

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
