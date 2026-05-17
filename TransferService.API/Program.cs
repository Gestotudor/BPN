using BuildingBlocks.Startup;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TransferService.API.Extensions;
using TransferService.Application;
using TransferService.Infrastructure;
using TransferService.Infrastructure.Persistence;

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
    var dbContext = services.GetRequiredService<TransferDbContext>();
    await dbContext.Database.MigrateAsync(cancellationToken);
});

app.UseTransferApiPipeline();

app.Run();
