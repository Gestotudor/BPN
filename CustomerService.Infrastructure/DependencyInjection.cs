using CustomerService.Application.Interfaces;
using CustomerService.Infrastructure.ExternalServices.Auth;
using CustomerService.Infrastructure.ExternalServices.Kyc;
using CustomerService.Infrastructure.ExternalServices.Transfer;
using CustomerService.Infrastructure.Persistence;
using CustomerService.Infrastructure.Repositories;
using CustomerService.Infrastructure.Services;
using CustomerService.Infrastructure.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CustomerDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("CustomerDb"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICustomerStatusHistoryRepository, CustomerStatusHistoryRepository>();
        services.AddScoped<IKycVerificationLogRepository, KycVerificationLogRepository>();
        services.AddSingleton<INationalIdValidator, TurkishNationalIdValidator>();

        services.AddHttpClient<IAuthServiceClient, AuthServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:AuthService:BaseUrl"]!);
        });

        services.AddHttpClient<IKycServiceClient, KycServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:KycService:BaseUrl"]!);
        });

        services.AddHttpClient<ITransferServiceClient, TransferServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:TransferService:BaseUrl"]!);
        });

        services.AddHealthChecks()
            .AddCheck("postgresql", new PostgresHealthCheck(configuration.GetConnectionString("CustomerDb")!))
            .AddCheck(
                "auth-service",
                new HttpDependencyHealthCheck(configuration["ExternalServices:AuthService:BaseUrl"]!, "Auth Service"))
            .AddCheck(
                "kyc-service",
                new KycServiceHealthCheck(configuration["ExternalServices:KycService:BaseUrl"]!))
            .AddCheck(
                "transfer-service",
                new HttpDependencyHealthCheck(configuration["ExternalServices:TransferService:BaseUrl"]!, "Transfer Service"));

        return services;
    }
}
