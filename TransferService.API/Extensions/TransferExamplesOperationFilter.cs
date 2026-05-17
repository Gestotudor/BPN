using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TransferService.API.Extensions;

public sealed class TransferExamplesOperationFilter : IOperationFilter
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
            case "api/transfers":
                ApplyCreateExamples(operation, context.ApiDescription.HttpMethod);
                break;
            case "api/transfers/receive":
                ApplyReceiveExamples(operation);
                break;
            case "api/transfers/{id:guid}/cancel":
                ApplyCancelExamples(operation);
                break;
            case "api/internal/customer-status-changed":
                ApplyCustomerStatusChangedExamples(operation);
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
            ["senderCustomer"] = CreateCustomerObject("Ali", "Yilmaz", "12345678910", "5551112233"),
            ["receiverCustomer"] = CreateCustomerObject("Ayse", "Demir", "10987654321", "5552223344"),
            ["amount"] = new OpenApiDouble(100),
            ["currency"] = new OpenApiString("USD")
        });

        SetResponseExample(operation, StatusCodes.Status200OK, new OpenApiObject
        {
            ["success"] = new OpenApiBoolean(true),
            ["message"] = new OpenApiNull(),
            ["data"] = new OpenApiObject
            {
                ["id"] = new OpenApiString("c1662d96-e0ae-4688-90f5-c7abdd7b8c2a"),
                ["transactionCode"] = new OpenApiString("MB12345678"),
                ["status"] = new OpenApiString("Pending"),
                ["amount"] = new OpenApiDouble(100),
                ["currency"] = new OpenApiString("USD"),
                ["tryAmount"] = new OpenApiDouble(3850),
                ["fee"] = new OpenApiDouble(15),
                ["fraudRiskLevel"] = new OpenApiString("Low"),
                ["exchangeRate"] = new OpenApiDouble(38.5),
                ["senderCustomerId"] = new OpenApiString("8edccf8d-536f-40ac-8f39-08b63405ca74"),
                ["receiverCustomerId"] = new OpenApiString("4b03714d-a6b5-4302-b708-af0ff7110714"),
                ["approvalAvailableAt"] = new OpenApiNull(),
                ["createdAt"] = new OpenApiString("2026-05-17T10:00:00Z"),
                ["completedAt"] = new OpenApiNull(),
                ["cancelledAt"] = new OpenApiNull()
            }
        });
    }

    private static void ApplyReceiveExamples(OpenApiOperation operation)
    {
        SetRequestExample(operation, new OpenApiObject
        {
            ["transactionCode"] = new OpenApiString("MB12345678"),
            ["receiverCustomerId"] = new OpenApiString("4b03714d-a6b5-4302-b708-af0ff7110714")
        });
    }

    private static void ApplyCancelExamples(OpenApiOperation operation)
    {
        SetRequestExample(operation, new OpenApiObject
        {
            ["reason"] = new OpenApiString("Customer request")
        });
    }

    private static void ApplyCustomerStatusChangedExamples(OpenApiOperation operation)
    {
        SetRequestExample(operation, new OpenApiObject
        {
            ["customerId"] = new OpenApiString("8edccf8d-536f-40ac-8f39-08b63405ca74"),
            ["oldStatus"] = new OpenApiString("Active"),
            ["newStatus"] = new OpenApiString("Blocked")
        });
    }

    private static OpenApiObject CreateCustomerObject(string name, string surname, string nationalIdNumber, string phoneNumber)
    {
        return new OpenApiObject
        {
            ["name"] = new OpenApiString(name),
            ["surname"] = new OpenApiString(surname),
            ["nationalIdNumber"] = new OpenApiString(nationalIdNumber),
            ["taxNumber"] = new OpenApiNull(),
            ["phoneNumber"] = new OpenApiString(phoneNumber),
            ["dateOfBirth"] = new OpenApiString("1990-05-10"),
            ["type"] = new OpenApiString("Individual")
        };
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
