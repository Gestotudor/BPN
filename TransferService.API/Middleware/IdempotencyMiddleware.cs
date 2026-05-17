using BuildingBlocks.Responses;

namespace TransferService.API.Middleware;

public sealed class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (HttpMethods.IsPost(context.Request.Method) &&
            context.Request.Path.Equals("/api/transfers", StringComparison.OrdinalIgnoreCase) &&
            !context.Request.Headers.ContainsKey("Idempotency-Key"))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail("Idempotency-Key header is required."));
            return;
        }

        await _next(context);
    }
}
