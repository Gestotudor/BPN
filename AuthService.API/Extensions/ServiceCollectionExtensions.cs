using AuthService.Application.Common.Results;
using BuildingBlocks.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AuthService.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
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
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MoneyBee Auth API",
                Version = "v1",
                Description = "Authentication and API key management endpoints for MoneyBee internal services, branches, and client applications.",
                Contact = new OpenApiContact
                {
                    Name = "MoneyBee Platform Team",
                    Email = "platform@moneybee.local"
                },
                License = new OpenApiLicense
                {
                    Name = "Proprietary - Internal Use"
                }
            });

            foreach (var xmlFile in new[]
                     {
                         $"{Assembly.GetExecutingAssembly().GetName().Name}.xml",
                         "AuthService.Application.xml",
                         "AuthService.Domain.xml",
                         "BuildingBlocks.xml"
                     })
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            }

            options.SchemaFilter<AuthEnumSchemaFilter>();
            options.OperationFilter<AuthExamplesOperationFilter>();

            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API key authentication using the `X-API-Key` header.",
                Type = SecuritySchemeType.ApiKey,
                Name = "X-API-Key",
                In = ParameterLocation.Header,
                Scheme = "ApiKeyScheme"
            });

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
