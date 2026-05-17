using AuthService.Application.Common.Results;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using MediatR;

namespace AuthService.Application.Features.ApiKeys.Commands.CreateApiKey;

public sealed class CreateApiKeyCommandHandler
    : IRequestHandler<CreateApiKeyCommand, Result<CreateApiKeyResponse>>
{
    private static readonly IReadOnlyDictionary<string, string[]> ScopeDependencies =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["transfer.write"] = new[] { "customer.read", "customer.write" }
        };

    private readonly IApiClientRepository _apiClientRepository;
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IApiScopeRepository _apiScopeRepository;
    private readonly IApiKeyGenerator _apiKeyGenerator;

    public CreateApiKeyCommandHandler(
        IApiClientRepository apiClientRepository,
        IApiKeyRepository apiKeyRepository,
        IApiScopeRepository apiScopeRepository,
        IApiKeyGenerator apiKeyGenerator)
    {
        _apiClientRepository = apiClientRepository;
        _apiKeyRepository = apiKeyRepository;
        _apiScopeRepository = apiScopeRepository;
        _apiKeyGenerator = apiKeyGenerator;
    }

    public async Task<Result<CreateApiKeyResponse>> Handle(
        CreateApiKeyCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedClientName = request.ClientName.Trim();
        var normalizedScopes = ExpandScopes(request.Scopes)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var scopes = await _apiScopeRepository.GetByNamesAsync(normalizedScopes, cancellationToken);

        if (scopes.Count != normalizedScopes.Length)
        {
            return Result<CreateApiKeyResponse>.Failure(
                ResultErrorType.Validation,
                "One or more scopes are invalid.");
        }

        var client = await _apiClientRepository.GetByNameAsync(normalizedClientName, cancellationToken);
        var isNewClient = client is null;

        client ??= new ApiClient
        {
            Id = Guid.NewGuid(),
            Name = normalizedClientName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (isNewClient)
        {
            await _apiClientRepository.AddAsync(client, cancellationToken);
        }

        var generatedApiKey = _apiKeyGenerator.Generate();

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            ClientId = client.Id,
            Client = client,
            KeyPrefix = generatedApiKey.KeyPrefix,
            KeyHash = generatedApiKey.KeyHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt
        };

        foreach (var scope in scopes)
        {
            apiKey.ApiKeyScopes.Add(new ApiKeyScope
            {
                ApiKeyId = apiKey.Id,
                ApiKey = apiKey,
                ScopeId = scope.Id,
                Scope = scope
            });
        }

        await _apiKeyRepository.AddAsync(apiKey, cancellationToken);
        await _apiKeyRepository.SaveChangesAsync(cancellationToken);

        return Result<CreateApiKeyResponse>.Success(
            new CreateApiKeyResponse(
                apiKey.Id,
                client.Name,
                generatedApiKey.PlainTextKey,
                apiKey.CreatedAt,
                scopes.Select(x => x.Name).OrderBy(x => x).ToArray()));
    }

    private static IReadOnlyCollection<string> ExpandScopes(IReadOnlyCollection<string> requestedScopes)
    {
        var expanded = new HashSet<string>(
            requestedScopes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim()),
            StringComparer.OrdinalIgnoreCase);

        var queue = new Queue<string>(expanded);
        while (queue.Count > 0)
        {
            var scope = queue.Dequeue();
            if (!ScopeDependencies.TryGetValue(scope, out var dependentScopes))
            {
                continue;
            }

            foreach (var dependentScope in dependentScopes)
            {
                if (expanded.Add(dependentScope))
                {
                    queue.Enqueue(dependentScope);
                }
            }
        }

        return expanded.ToArray();
    }
}
