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
            app.UseSwaggerUI(options =>
            {
                options.DocumentTitle = "MoneyBee Transfer API";
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "MoneyBee Transfer API v1");
                options.DisplayRequestDuration();
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                options.DefaultModelsExpandDepth(-1);
            });
        }

        app.UseHttpsRedirection();
        app.MapHealthChecks("/health");
        app.MapControllers();

        return app;
    }
}
