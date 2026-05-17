namespace AuthService.Application.Features.ApiKeys.Commands.CreateApiKey;

public sealed record CreateApiKeyResponse(
    Guid Id,
    string ClientName,
    string ApiKey,
    DateTime CreatedAt,
    IReadOnlyCollection<string> Scopes);
