using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransferService.Application.Interfaces;
using TransferService.Infrastructure.ExternalServices.Auth;
using TransferService.Infrastructure.ExternalServices.Customer;
using TransferService.Infrastructure.ExternalServices.Exchange;
using TransferService.Infrastructure.ExternalServices.Fraud;
using TransferService.Infrastructure.Generators;
using TransferService.Infrastructure.Persistence;
using TransferService.Infrastructure.Repositories;
using TransferService.Infrastructure.Services;

namespace TransferService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TransferDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("TransferDb"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
        });

        services.AddScoped<ITransferRepository, TransferRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        services.AddScoped<ITransferStatusHistoryRepository, TransferStatusHistoryRepository>();
        services.AddScoped<ITransferFeeRefundRepository, TransferFeeRefundRepository>();
        services.AddScoped<ITransferFeeCalculator, TransferFeeCalculator>();
        services.AddScoped<ITransactionCodeGenerator, TransactionCodeGenerator>();

        services.AddHttpClient<IAuthServiceClient, AuthServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:AuthService:BaseUrl"]!);
        });

        services.AddHttpClient<ICustomerServiceClient, CustomerServiceClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:CustomerService:BaseUrl"]!);
        });

        services.AddHttpClient<IExchangeRateClient, ExchangeRateClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:ExchangeService:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHttpClient<IFraudDetectionClient, FraudDetectionClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["ExternalServices:FraudService:BaseUrl"]!);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddHealthChecks()
            .AddCheck("postgresql", new PostgresHealthCheck(configuration.GetConnectionString("TransferDb")!))
            .AddCheck("fraud-service", new HttpDependencyHealthCheck(configuration["ExternalServices:FraudService:BaseUrl"]!, "/api/fraud/health"))
            .AddCheck("exchange-service", new HttpDependencyHealthCheck(configuration["ExternalServices:ExchangeService:BaseUrl"]!, "/api/health"))
            .AddCheck("customer-service", new HttpDependencyHealthCheck(configuration["ExternalServices:CustomerService:BaseUrl"]!, "/health"))
            .AddCheck("auth-service", new HttpDependencyHealthCheck(configuration["ExternalServices:AuthService:BaseUrl"]!, "/health"));

        return services;
    }
}
