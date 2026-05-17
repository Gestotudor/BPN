namespace AuthService.Application.Features.Auth.Queries.ValidateApiKey;

public sealed record ValidateApiKeyRequest(string ApiKey, IReadOnlyCollection<string>? RequiredScopes);
