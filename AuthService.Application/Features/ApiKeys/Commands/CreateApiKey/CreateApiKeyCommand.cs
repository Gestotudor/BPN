using AuthService.Application.Common.Results;
using MediatR;

namespace AuthService.Application.Features.ApiKeys.Commands.CreateApiKey;

/// <summary>
/// Request payload used to create an API key for a branch, internal service, or client application.
/// </summary>
/// <param name="ClientName">Display name of the API client that will own the key.</param>
/// <param name="Scopes">Scopes granted to the newly created key.</param>
/// <param name="ExpiresAt">Optional UTC expiration date for the key.</param>
public sealed record CreateApiKeyCommand(
    string ClientName,
    IReadOnlyCollection<string> Scopes,
    DateTime? ExpiresAt) : IRequest<Result<CreateApiKeyResponse>>;
