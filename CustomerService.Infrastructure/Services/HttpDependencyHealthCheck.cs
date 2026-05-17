using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CustomerService.Infrastructure.Services;

public sealed class HttpDependencyHealthCheck : IHealthCheck
{
    private readonly string _baseUrl;
    private readonly string _name;
    private readonly string _healthPath;

    public HttpDependencyHealthCheck(string baseUrl, string name, string healthPath = "/health")
    {
        _baseUrl = baseUrl;
        _name = name;
        _healthPath = healthPath;
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

            var response = await httpClient.GetAsync(_healthPath, cancellationToken);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"{_name} is healthy.")
                : HealthCheckResult.Unhealthy($"{_name} returned status code {response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"{_name} is unavailable.", ex);
        }
    }
}
