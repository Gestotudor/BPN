using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.RateLimiting;
using AuthService.Infrastructure.Repositories;
using AuthService.Infrastructure.Security;
using AuthService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("AuthDb"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
        });

        services.AddMemoryCache();
        services.AddScoped<IApiClientRepository, ApiClientRepository>();
        services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        services.AddScoped<IApiScopeRepository, ApiScopeRepository>();
        services.AddSingleton<IApiKeyGenerator, ApiKeyGenerator>();
        services.AddSingleton<IRateLimitService, MemoryRateLimitService>();
        services.AddScoped<AuthDbContextSeeder>();

        services.AddHealthChecks()
            .AddCheck(
                "postgresql",
                new PostgresHealthCheck(configuration.GetConnectionString("AuthDb")!));

        return services;
    }
}
