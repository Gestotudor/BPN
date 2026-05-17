using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TransferService.Infrastructure.Services;

public sealed class HttpDependencyHealthCheck : IHealthCheck
{
    private readonly string _baseUrl;
    private readonly string _path;

    public HttpDependencyHealthCheck(string baseUrl, string path)
    {
        _baseUrl = baseUrl;
        _path = path;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(_baseUrl), Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync(_path, cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Dependency returned status {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(exception: ex);
        }
    }
}
