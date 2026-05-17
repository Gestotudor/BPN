using BuildingBlocks.Startup;
using AuthService.API.Extensions;
using AuthService.API.Middleware;
using AuthService.Application;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices();

var app = builder.Build();

await app.Services.RunStartupTaskWithRetryAsync(async (services, cancellationToken) =>
{
    var seeder = services.GetRequiredService<AuthDbContextSeeder>();
    await seeder.SeedAsync(cancellationToken);
});

app.UseApiPipeline();

app.Run();
