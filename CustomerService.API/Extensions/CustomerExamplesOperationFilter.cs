using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CustomerService.API.Extensions;

public sealed class CustomerExamplesOperationFilter : IOperationFilter
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
            case "api/customers":
                ApplyCreateExamples(operation, context.ApiDescription.HttpMethod);
                break;
            case "api/customers/{id:guid}/status":
                ApplyStatusExamples(operation);
                break;
            case "api/customers/validate":
                ApplyValidateExamples(operation);
                break;
        }
    }

    private static void ApplyCreateExamples(OpenApiOperation operation, string? httpMethod)
    {
        if (!string.Equals(httpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SetRequestExample(operation, new OpenApiObject
        {
            ["name"] = new OpenApiString("Ali"),
            ["surname"] = new OpenApiString("Yilmaz"),
            ["nationalIdNumber"] = new OpenApiString("12345678910"),
            ["taxNumber"] = new OpenApiNull(),
            ["phoneNumber"] = new OpenApiString("5551112233"),
            ["dateOfBirth"] = new OpenApiString("1990-05-10"),
            ["type"] = new OpenApiString("Individual")
        });

        SetResponseExample(operation, StatusCodes.Status200OK, new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiNull(),
            ["data"] = new OpenApiObject
            {
                ["id"] = new OpenApiString("8edccf8d-536f-40ac-8f39-08b63405ca74"),
                ["name"] = new OpenApiString("Ali"),
                ["surname"] = new OpenApiString("Yilmaz"),
                ["fullName"] = new OpenApiString("Ali Yilmaz"),
                ["nationalIdNumber"] = new OpenApiString("12345678910"),
                ["taxNumber"] = new OpenApiNull(),
                ["phoneNumber"] = new OpenApiString("5551112233"),
                ["dateOfBirth"] = new OpenApiString("1990-05-10T00:00:00Z"),
                ["type"] = new OpenApiString("Individual"),
                ["status"] = new OpenApiString("Active"),
                ["isKycVerified"] = new OpenApiBoolean(true),
                ["kycVerifiedAt"] = new OpenApiString("2026-05-17T10:00:00Z"),
                ["createdAt"] = new OpenApiString("2026-05-17T10:00:00Z"),
                ["updatedAt"] = new OpenApiNull()
            }
        });
    }

    private static void ApplyStatusExamples(OpenApiOperation operation)
    {
        SetRequestExample(operation, new OpenApiObject
        {
            ["status"] = new OpenApiString("Blocked"),
            ["reason"] = new OpenApiString("Fraud suspicion")
        });
    }

    private static void ApplyValidateExamples(OpenApiOperation operation)
    {
        SetRequestExample(operation, new OpenApiObject
        {
            ["customerId"] = new OpenApiString("8edccf8d-536f-40ac-8f39-08b63405ca74")
        });

        SetResponseExample(operation, StatusCodes.Status200OK, new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiNull(),
            ["data"] = new OpenApiObject
            {
                ["isValid"] = new OpenApiBoolean(true),
                ["customerId"] = new OpenApiString("8edccf8d-536f-40ac-8f39-08b63405ca74"),
                ["status"] = new OpenApiString("Active"),
                ["isKycVerified"] = new OpenApiBoolean(true),
                ["fullName"] = new OpenApiString("Ali Yilmaz")
            }
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
