using Serilog;
using TransferService.API.Middleware;

namespace TransferService.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseTransferApiPipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<IdempotencyMiddleware>();
        app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.MapHealthChecks("/health");
        app.MapControllers();

        return app;
    }
}
