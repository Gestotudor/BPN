using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthService.API.Extensions;

public sealed class AuthExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        switch (path)
        {
            case "api/api-keys":
                ApplyApiKeysExamples(operation, context.ApiDescription.HttpMethod);
                break;
            case "api/auth/validate":
                ApplyValidateExamples(operation);
                break;
            case "api/api-keys/{id:guid}":
                ApplyRevokeExamples(operation);
                break;
        }
    }

    private static void ApplyApiKeysExamples(OpenApiOperation operation, string? httpMethod)
    {
        if (string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["clientName"] = new OpenApiString("Eskisehir Branch"),
                ["scopes"] = new OpenApiArray
                {
                    new OpenApiString("customer.read"),
                    new OpenApiString("customer.write"),
                    new OpenApiString("transfer.read"),
                    new OpenApiString("transfer.write")
                },
                ["expiresAt"] = new OpenApiNull()
            });

            SetResponseExample(operation, StatusCodes.Status200OK, new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(true),
                ["message"] = new OpenApiString("API key created successfully."),
                ["data"] = new OpenApiObject
                {
                    ["id"] = new OpenApiString("7c1d3d58-4e6b-4a0c-b1b7-4a4fdc8e2a11"),
                    ["clientName"] = new OpenApiString("Eskisehir Branch"),
                    ["apiKey"] = new OpenApiString("mb_live_xxxxxxxxxxxxxxxxx"),
                    ["createdAt"] = new OpenApiString("2026-05-17T10:00:00Z"),
                    ["scopes"] = new OpenApiArray
                    {
                        new OpenApiString("customer.read"),
                        new OpenApiString("customer.write"),
                        new OpenApiString("transfer.read"),
                        new OpenApiString("transfer.write")
                    }
                }
            });
        }
        else if (string.Equals(httpMethod, "GET", StringComparison.OrdinalIgnoreCase))
        {
            SetResponseExample(operation, StatusCodes.Status200OK, new OpenApiObject
            {
                ["success"] = new OpenApiBoolean(true),
                ["message"] = new OpenApiNull(),
                ["data"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("7c1d3d58-4e6b-4a0c-b1b7-4a4fdc8e2a11"),
                        ["clientId"] = new OpenApiString("f1a4fe4b-c228-40c9-b3be-bdb74f9eaf1d"),
                        ["clientName"] = new OpenApiString("Eskisehir Branch"),
                        ["keyPrefix"] = new OpenApiString("mb_live_"),
                        ["isActive"] = new OpenApiBoolean(true),
                        ["createdAt"] = new OpenApiString("2026-05-17T10:00:00Z"),
                        ["expiresAt"] = new OpenApiNull(),
                        ["revokedAt"] = new OpenApiNull(),
                        ["lastUsedAt"] = new OpenApiString("2026-05-17T12:30:00Z"),
                        ["scopes"] = new OpenApiArray
                        {
                            new OpenApiString("customer.read"),
                            new OpenApiString("customer.write")
                        }
                    }
                }
            });
        }
    }

    private static void ApplyValidateExamples(OpenApiOperation operation)
    {
        SetRequestExample(operation, new OpenApiObject
        {
            ["apiKey"] = new OpenApiString("mb_live_xxxxxxxxxxxxxxxxx"),
            ["requiredScopes"] = new OpenApiArray
            {
                new OpenApiString("transfer.write")
            }
        });

        SetResponseExample(operation, StatusCodes.Status200OK, new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiNull(),
            ["data"] = new OpenApiObject
            {
                ["isValid"] = new OpenApiBoolean(true),
                ["isAuthorized"] = new OpenApiBoolean(true),
                ["clientId"] = new OpenApiString("7c1d3d58-4e6b-4a0c-b1b7-4a4fdc8e2a11"),
                ["clientName"] = new OpenApiString("Eskisehir Branch"),
                ["scopes"] = new OpenApiArray
                {
                    new OpenApiString("transfer.read"),
                    new OpenApiString("transfer.write")
                }
            }
        });
    }

    private static void ApplyRevokeExamples(OpenApiOperation operation)
    {
        SetResponseExample(operation, StatusCodes.Status200OK, new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiString("API key revoked successfully."),
            ["data"] = new OpenApiObject()
        });
    }

    private static void SetRequestExample(OpenApiOperation operation, IOpenApiAny example)
    {
        var content = operation.RequestBody?.Content;
        if (content is not null && content.TryGetValue("application/json", out var mediaType))
        {
            mediaType.Example = example;
        }
    }

    private static void SetResponseExample(OpenApiOperation operation, int statusCode, IOpenApiAny example)
    {
        if (operation.Responses.TryGetValue(statusCode.ToString(), out var response) &&
            response.Content.TryGetValue("application/json", out var mediaType))
        {
            mediaType.Example = example;
        }
    }
}
