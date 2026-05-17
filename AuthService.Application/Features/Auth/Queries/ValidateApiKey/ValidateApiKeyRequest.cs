namespace AuthService.Application.Features.Auth.Queries.ValidateApiKey;

/// <summary>
/// Request payload used to validate an API key and optionally check required scopes.
/// </summary>
/// <param name="ApiKey">Plaintext API key sent by the caller.</param>
/// <param name="RequiredScopes">Optional scopes that must be granted for the request to be authorized.</param>
public sealed record ValidateApiKeyRequest(string ApiKey, IReadOnlyCollection<string>? RequiredScopes);
