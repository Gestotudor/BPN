using BuildingBlocks.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using TransferService.Application.Common.Results;

namespace TransferService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            x => x.Key,
                            x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                    var response = ApiResponse<object>.Fail("Validation failed.");
                    response.Data = errors;
                    return new BadRequestObjectResult(response);
                };
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "MoneyBee Transfer API", Version = "v1" });
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key authentication using the X-API-Key header.",
                Type = SecuritySchemeType.ApiKey,
                Name = "X-API-Key",
                In = ParameterLocation.Header
            });

            options.OperationFilter<SwaggerIdempotencyHeaderOperationFilter>();
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(ApiResponse<T>.Ok(result.Value!));
        }

        return result.ErrorType switch
        {
            ResultErrorType.NotFound => controller.NotFound(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            ResultErrorType.Validation => controller.BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            ResultErrorType.Conflict => controller.Conflict(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            ResultErrorType.Forbidden => controller.StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            ResultErrorType.ServiceUnavailable => controller.StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            _ => controller.BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors)))
        };
    }
}
