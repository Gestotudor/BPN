using System.Net.Http.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CustomerService.Infrastructure.ExternalServices.Kyc;

public sealed class KycServiceHealthCheck : IHealthCheck
{
    private readonly string _baseUrl;

    public KycServiceHealthCheck(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };

            var response = await httpClient.GetAsync("/health", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    $"KYC Service returned status code {response.StatusCode}.");
            }

            var content = await response.Content.ReadFromJsonAsync<KycHealthResponse>(
                cancellationToken: cancellationToken);

            if (content is null)
            {
                return HealthCheckResult.Unhealthy("KYC Service health response is empty.");
            }

            var isHealthy = string.Equals(content.Status, "healthy", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(content.Status, "ok", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(content.Status, "up", StringComparison.OrdinalIgnoreCase);

            return isHealthy
                ? HealthCheckResult.Healthy("KYC Service is healthy.")
                : HealthCheckResult.Unhealthy($"KYC Service reported status '{content.Status}'.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("KYC Service is unavailable.", ex);
        }
    }
}
