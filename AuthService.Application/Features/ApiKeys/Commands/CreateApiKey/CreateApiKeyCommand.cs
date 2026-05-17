using AuthService.Application.Common.Results;
using MediatR;

namespace AuthService.Application.Features.ApiKeys.Commands.CreateApiKey;

public sealed record CreateApiKeyCommand(
    string ClientName,
    IReadOnlyCollection<string> Scopes,
    DateTime? ExpiresAt) : IRequest<Result<CreateApiKeyResponse>>;
