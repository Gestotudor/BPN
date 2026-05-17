using System.Net;
using System.Net.Http.Json;
using BuildingBlocks.Responses;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Middleware
{
    public class ApiKeyAuthenticationMiddleware
    {
        private const string ApiKeyHeaderName = "X-API-Key";

        private readonly RequestDelegate _next;
        private readonly HttpClient _httpClient;

        public ApiKeyAuthenticationMiddleware(
            RequestDelegate next,
            IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _httpClient = httpClientFactory.CreateClient("AuthService");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            if (!TryReadApiKey(context, out var apiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail("API key is missing."));
                return;
            }

            var response = await _httpClient.PostAsJsonAsync(
                "/api/auth/validate",
                new ValidateApiKeyRequest(apiKey, ResolveRequiredScopes(context)),
                context.RequestAborted);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail("Rate limit exceeded."));
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail("API key validation failed."));
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<ApiKeyValidationResponse>(
                cancellationToken: context.RequestAborted);

            if (result?.Data?.IsValid != true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail("Invalid API key."));
                return;
            }

            if (result.Data.IsAuthorized != true)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(
                    ApiResponse<object>.Fail("API key does not have the required scope."));
                return;
            }

            context.Items["AuthenticatedClient"] = result.Data;

            await _next(context);
        }

        private static bool TryReadApiKey(HttpContext context, out string apiKey)
        {
            if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeader) &&
                !string.IsNullOrWhiteSpace(apiKeyHeader))
            {
                apiKey = apiKeyHeader.ToString();
                return true;
            }

            if (context.Request.Headers.TryGetValue("X-Api-Key", out apiKeyHeader) &&
                !string.IsNullOrWhiteSpace(apiKeyHeader))
            {
                apiKey = apiKeyHeader.ToString();
                return true;
            }

            apiKey = string.Empty;
            return false;
        }

        private static IReadOnlyCollection<string> ResolveRequiredScopes(HttpContext context)
        {
            return context.GetEndpoint()?
                .Metadata
                .GetMetadata<RequiredScopesAttribute>()?
                .Scopes ?? Array.Empty<string>();
        }
    }

    public class ApiKeyValidationResponse
    {
        public bool Success { get; set; }

        public ApiKeyValidationData? Data { get; set; }
    }

    public class ApiKeyValidationData
    {
        public bool IsValid { get; set; }

        public bool IsAuthorized { get; set; }

        public Guid? ClientId { get; set; }

        public string? ClientName { get; set; }

        public IReadOnlyCollection<string> Scopes { get; set; } = Array.Empty<string>();
    }

    public sealed record ValidateApiKeyRequest(string ApiKey, IReadOnlyCollection<string>? RequiredScopes);
}
