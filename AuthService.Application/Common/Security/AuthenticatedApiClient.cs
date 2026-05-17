namespace AuthService.Application.Common.Security;

public sealed record AuthenticatedApiClient(
    Guid ClientId,
    string ClientName,
    IReadOnlyCollection<string> Scopes,
    Guid ApiKeyId);
