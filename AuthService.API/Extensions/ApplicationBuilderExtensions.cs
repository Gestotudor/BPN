using AuthService.API.Middleware;
using Serilog;

namespace AuthService.API.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseApiPipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
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
