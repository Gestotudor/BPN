using AuthService.Application.Common.Results;
using AuthService.Application.Interfaces;
using MediatR;

namespace AuthService.Application.Features.ApiKeys.Queries.GetApiKeys;

public sealed class GetApiKeysQueryHandler
    : IRequestHandler<GetApiKeysQuery, Result<IReadOnlyCollection<GetApiKeysResponse>>>
{
    private readonly IApiKeyRepository _apiKeyRepository;

    public GetApiKeysQueryHandler(IApiKeyRepository apiKeyRepository)
    {
        _apiKeyRepository = apiKeyRepository;
    }

    public async Task<Result<IReadOnlyCollection<GetApiKeysResponse>>> Handle(
        GetApiKeysQuery request,
        CancellationToken cancellationToken)
    {
        var apiKeys = await _apiKeyRepository.GetActiveAsync(cancellationToken);

        var response = apiKeys
            .Select(apiKey => new GetApiKeysResponse(
                apiKey.Id,
                apiKey.ClientId,
                apiKey.Client.Name,
                apiKey.KeyPrefix,
                apiKey.IsActive,
                apiKey.CreatedAt,
                apiKey.ExpiresAt,
                apiKey.RevokedAt,
                apiKey.LastUsedAt,
                apiKey.ApiKeyScopes.Select(x => x.Scope.Name).OrderBy(x => x).ToArray()))
            .ToArray();

        return Result<IReadOnlyCollection<GetApiKeysResponse>>.Success(response);
    }
}
