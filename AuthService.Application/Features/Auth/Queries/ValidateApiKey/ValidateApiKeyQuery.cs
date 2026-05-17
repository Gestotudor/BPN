using AuthService.Application.Common.Results;
using MediatR;

namespace AuthService.Application.Features.Auth.Queries.ValidateApiKey;

public sealed record ValidateApiKeyQuery(
    string ApiKey,
    IReadOnlyCollection<string>? RequiredScopes) : IRequest<Result<ValidateApiKeyResponse>>;
