using AuthService.Application.Common.Results;
using MediatR;

namespace AuthService.Application.Features.ApiKeys.Queries.GetApiKeys;

public sealed record GetApiKeysQuery() : IRequest<Result<IReadOnlyCollection<GetApiKeysResponse>>>;
