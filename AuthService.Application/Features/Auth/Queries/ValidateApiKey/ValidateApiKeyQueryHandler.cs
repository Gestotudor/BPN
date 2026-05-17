using AuthService.Application.Common.Results;
using AuthService.Application.Interfaces;
using MediatR;

namespace AuthService.Application.Features.Auth.Queries.ValidateApiKey;

public sealed class ValidateApiKeyQueryHandler
    : IRequestHandler<ValidateApiKeyQuery, Result<ValidateApiKeyResponse>>
{
    private static readonly IReadOnlyDictionary<string, string[]> ScopeDependencies =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["transfer.write"] = new[] { "customer.read", "customer.write" }
        };

    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IApiKeyGenerator _apiKeyGenerator;
    private readonly IRateLimitService _rateLimitService;

    public ValidateApiKeyQueryHandler(
        IApiKeyRepository apiKeyRepository,
        IApiKeyGenerator apiKeyGenerator,
        IRateLimitService rateLimitService)
    {
        _apiKeyRepository = apiKeyRepository;
        _apiKeyGenerator = apiKeyGenerator;
        _rateLimitService = rateLimitService;
    }

    public async Task<Result<ValidateApiKeyResponse>> Handle(
        ValidateApiKeyQuery request,
        CancellationToken cancellationToken)
    {
        var isAllowed = await _rateLimitService.IsRequestAllowedAsync(request.ApiKey, cancellationToken);

        if (!isAllowed)
        {
            return Result<ValidateApiKeyResponse>.Failure(
                ResultErrorType.Failure,
                "Rate limit exceeded.");
        }

        var keyHash = _apiKeyGenerator.ComputeHash(request.ApiKey);
        var apiKey = await _apiKeyRepository.GetByHashAsync(keyHash, cancellationToken);

        if (apiKey is null ||
            !apiKey.IsActive ||
            apiKey.RevokedAt.HasValue ||
            (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value <= DateTime.UtcNow) ||
            !apiKey.Client.IsActive)
        {
            return Result<ValidateApiKeyResponse>.Success(
                new ValidateApiKeyResponse(false, false, null, null, Array.Empty<string>()));
        }

        var grantedScopes = ExpandScopes(apiKey.ApiKeyScopes
            .Select(x => x.Scope.Name)
            .OrderBy(x => x)
            .ToArray());

        var normalizedRequiredScopes = request.RequiredScopes?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

        var isAuthorized = normalizedRequiredScopes.All(
            requiredScope => grantedScopes.Contains(requiredScope, StringComparer.OrdinalIgnoreCase));

        apiKey.LastUsedAt = DateTime.UtcNow;
        apiKey.Client.UpdatedAt = DateTime.UtcNow;

        await _apiKeyRepository.SaveChangesAsync(cancellationToken);

        return Result<ValidateApiKeyResponse>.Success(
            new ValidateApiKeyResponse(
                true,
                isAuthorized,
                apiKey.ClientId,
                apiKey.Client.Name,
                grantedScopes));
    }

    private static string[] ExpandScopes(IReadOnlyCollection<string> assignedScopes)
    {
        var expanded = new HashSet<string>(
            assignedScopes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()),
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

        return expanded.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
