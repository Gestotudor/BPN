using AuthService.Application.Common.Results;
using MediatR;

namespace AuthService.Application.Features.ApiKeys.Commands.RevokeApiKey;

public sealed record RevokeApiKeyCommand(Guid Id, string? Reason) : IRequest<Result>;
