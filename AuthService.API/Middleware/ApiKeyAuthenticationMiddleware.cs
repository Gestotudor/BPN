using AuthService.Application.Common.Security;
using AuthService.Application.Interfaces;
using BuildingBlocks.Responses;
using Microsoft.AspNetCore.Http;

namespace AuthService.API.Middleware;

public sealed class ApiKeyAuthenticationMiddleware
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IApiKeyRepository apiKeyRepository,
        IApiKeyGenerator apiKeyGenerator,
        IRateLimitService rateLimitService)
    {
        if (IsAnonymousPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (!TryReadApiKey(context, out var apiKey))
        {
            _logger.LogWarning("Missing API key for path {Path}", context.Request.Path);
            await WriteResponseAsync(context, StatusCodes.Status401Unauthorized, "API key is missing.");
            return;
        }

        var keyHash = apiKeyGenerator.ComputeHash(apiKey);
        var storedApiKey = await apiKeyRepository.GetByHashAsync(keyHash, context.RequestAborted);

        if (storedApiKey is null ||
            !storedApiKey.IsActive ||
            storedApiKey.RevokedAt.HasValue ||
            (storedApiKey.ExpiresAt.HasValue && storedApiKey.ExpiresAt.Value <= DateTime.UtcNow) ||
            !storedApiKey.Client.IsActive)
        {
            _logger.LogWarning("Invalid API key attempt for path {Path}", context.Request.Path);
            await WriteResponseAsync(context, StatusCodes.Status401Unauthorized, "Invalid API key.");
            return;
        }

        var isAllowed = await rateLimitService.IsRequestAllowedAsync(apiKey, context.RequestAborted);

        if (!isAllowed)
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientName}", storedApiKey.Client.Name);
            await WriteResponseAsync(context, StatusCodes.Status429TooManyRequests, "Rate limit exceeded.");
            return;
        }

        storedApiKey.LastUsedAt = DateTime.UtcNow;
        await apiKeyRepository.SaveChangesAsync(context.RequestAborted);

        context.Items["AuthenticatedClient"] = new AuthenticatedApiClient(
            storedApiKey.ClientId,
            storedApiKey.Client.Name,
            storedApiKey.ApiKeyScopes.Select(x => x.Scope.Name).OrderBy(x => x).ToArray(),
            storedApiKey.Id);

        await _next(context);
    }

    private static bool IsAnonymousPath(PathString path)
    {
        return path.StartsWithSegments("/swagger") ||
               path.StartsWithSegments("/health") ||
               path.StartsWithSegments("/api/auth/validate");
    }

    private static bool TryReadApiKey(HttpContext context, out string apiKey)
    {
        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            apiKey = headerValue.ToString();
            return true;
        }

        if (context.Request.Headers.TryGetValue("X-Api-Key", out headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            apiKey = headerValue.ToString();
            return true;
        }

        apiKey = string.Empty;
        return false;
    }

    private static async Task WriteResponseAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(message));
    }
}
