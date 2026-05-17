using System.Net;
using System.Text.Json;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BusinessException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.BadRequest);
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(context, ex, HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred.");

            await HandleExceptionAsync(
                context,
                ex,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.");
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        HttpStatusCode statusCode,
        string? customMessage = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(customMessage ?? exception.Message);

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}
