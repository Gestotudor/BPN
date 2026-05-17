using System.Net;
using System.Net.Http.Json;
using CustomerService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CustomerService.Infrastructure.ExternalServices.Auth;

public sealed class AuthServiceClient : IAuthServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthServiceClient> _logger;

    public AuthServiceClient(HttpClient httpClient, ILogger<AuthServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthValidationResult> ValidateAsync(
        string apiKey,
        IReadOnlyCollection<string> requiredScopes,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/api/auth/validate",
            new AuthValidateRequest(apiKey, requiredScopes),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return new AuthValidationResult(false, false, null, null, Array.Empty<string>());
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return new AuthValidationResult(true, false, null, null, Array.Empty<string>());
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Auth service validation failed with status code {StatusCode}", response.StatusCode);
            return new AuthValidationResult(false, false, null, null, Array.Empty<string>());
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthValidateResponse>>(cancellationToken: cancellationToken);
        var data = result?.Data;

        return new AuthValidationResult(
            data?.IsValid == true,
            data?.IsAuthorized == true,
            data?.ClientId,
            data?.ClientName,
            data?.Scopes ?? Array.Empty<string>());
    }

    private sealed record AuthValidateRequest(string ApiKey, IReadOnlyCollection<string> RequiredScopes);

    private sealed record ApiResponse<T>(bool Success, string? Message, T? Data);

    private sealed record AuthValidateResponse(
        bool IsValid,
        bool IsAuthorized,
        Guid? ClientId,
        string? ClientName,
        IReadOnlyCollection<string> Scopes);
}
