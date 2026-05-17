using BuildingBlocks.Middleware;
using BuildingBlocks.Responses;
using TransferService.Application.Interfaces;

namespace TransferService.API.Middleware;

public sealed class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

    public ApiKeyAuthenticationMiddleware(RequestDelegate next, ILogger<ApiKeyAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthServiceClient authServiceClient)
    {
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        if (!TryReadApiKey(context, out var apiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("API key is missing."));
            return;
        }

        var requiredScopes = context.GetEndpoint()?
            .Metadata
            .GetMetadata<RequiredScopesAttribute>()?
            .Scopes ?? Array.Empty<string>();

        var result = await authServiceClient.ValidateAsync(apiKey, requiredScopes, context.RequestAborted);
        if (!result.IsValid)
        {
            _logger.LogWarning("API key validation failed for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Invalid API key."));
            return;
        }

        if (!result.IsAuthorized)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Required scope is missing."));
            return;
        }

        await _next(context);
    }

    private static bool TryReadApiKey(HttpContext context, out string apiKey)
    {
        if (context.Request.Headers.TryGetValue("X-API-Key", out var value) && !string.IsNullOrWhiteSpace(value))
        {
            apiKey = value.ToString();
            return true;
        }

        if (context.Request.Headers.TryGetValue("X-Api-Key", out value) && !string.IsNullOrWhiteSpace(value))
        {
            apiKey = value.ToString();
            return true;
        }

        apiKey = string.Empty;
        return false;
    }
}
