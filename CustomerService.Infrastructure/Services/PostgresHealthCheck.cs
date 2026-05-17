using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace CustomerService.Infrastructure.Services;

public sealed class PostgresHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public PostgresHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return HealthCheckResult.Healthy("PostgreSQL connection is healthy.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("PostgreSQL connection failed.", ex);
        }
    }
}
