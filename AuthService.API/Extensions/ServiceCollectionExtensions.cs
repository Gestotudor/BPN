using AuthService.Application.Common.Results;
using BuildingBlocks.Responses;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
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
            options.SwaggerDoc("v1", new() { Title = "MoneyBee Auth API", Version = "v1" });
            options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "API Key authentication using the X-API-Key header.",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Name = "X-API-Key",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Scheme = "ApiKeyScheme"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(ApiResponse<T>.Ok(result.Value!));
        }

        return result.ErrorType switch
        {
            ResultErrorType.Validation => Results.BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            ResultErrorType.NotFound => Results.NotFound(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            ResultErrorType.Unauthorized => Results.Unauthorized(),
            _ => Results.BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors)))
        };
    }

    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(ApiResponse<object>.Ok(new { }));
        }

        return result.ErrorType switch
        {
            ResultErrorType.Validation => Results.BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            ResultErrorType.NotFound => Results.NotFound(ApiResponse<object>.Fail(string.Join("; ", result.Errors))),
            ResultErrorType.Unauthorized => Results.Unauthorized(),
            _ => Results.BadRequest(ApiResponse<object>.Fail(string.Join("; ", result.Errors)))
        };
    }
}
