using BuildingBlocks.Responses;
using CustomerService.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

namespace CustomerService.API.Extensions;

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
                Title = "MoneyBee Customer API",
                Version = "v1",
                Description = "Customer registration, lookup, validation, and status management endpoints for MoneyBee.",
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
                         "CustomerService.Application.xml",
                         "CustomerService.Domain.xml",
                         "BuildingBlocks.xml"
                     })
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
                }
            }

            options.SchemaFilter<CustomerEnumSchemaFilter>();
            options.OperationFilter<CustomerExamplesOperationFilter>();

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
