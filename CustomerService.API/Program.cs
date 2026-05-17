using BuildingBlocks.Startup;
using CustomerService.API.Extensions;
using CustomerService.Application;
using CustomerService.Infrastructure;
using CustomerService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
    var dbContext = services.GetRequiredService<CustomerDbContext>();
    await dbContext.Database.MigrateAsync(cancellationToken);
});

app.UseCustomerApiPipeline();

app.Run();
