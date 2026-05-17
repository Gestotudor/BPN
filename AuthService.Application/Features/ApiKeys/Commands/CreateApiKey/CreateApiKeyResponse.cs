namespace AuthService.Application.Features.ApiKeys.Commands.CreateApiKey;

/// <summary>
/// Response payload returned after creating an API key.
/// </summary>
/// <param name="Id">Unique identifier of the stored API key record.</param>
/// <param name="ClientName">Display name of the API client that owns the key.</param>
/// <param name="ApiKey">Plaintext API key. This value is returned only once.</param>
/// <param name="CreatedAt">UTC timestamp when the key was created.</param>
/// <param name="Scopes">Scopes granted to the key.</param>
public sealed record CreateApiKeyResponse(
    Guid Id,
    string ClientName,
    string ApiKey,
    DateTime CreatedAt,
    IReadOnlyCollection<string> Scopes);
