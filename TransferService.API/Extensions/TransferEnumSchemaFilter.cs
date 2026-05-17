using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TransferService.Application.Features.Transfers.DTOs;
using TransferService.Domain.Enums;

namespace TransferService.API.Extensions;

public sealed class TransferEnumSchemaFilter : ISchemaFilter
{
    private static readonly IReadOnlyDictionary<Type, IReadOnlyDictionary<string, string>> EnumDescriptions =
        new Dictionary<Type, IReadOnlyDictionary<string, string>>
        {
            [typeof(TransferStatus)] = new Dictionary<string, string>
            {
                [nameof(TransferStatus.Pending)] = "Transfer created but not yet received.",
                [nameof(TransferStatus.Completed)] = "Receiver received the money.",
                [nameof(TransferStatus.Cancelled)] = "Transfer cancelled before receiving.",
                [nameof(TransferStatus.Failed)] = "Transfer rejected or failed."
            },
            [typeof(FraudRiskLevel)] = new Dictionary<string, string>
            {
                [nameof(FraudRiskLevel.Low)] = "Transfer can proceed.",
                [nameof(FraudRiskLevel.High)] = "Transfer must be rejected."
            },
            [typeof(TransferCustomerType)] = new Dictionary<string, string>
            {
                [nameof(TransferCustomerType.Individual)] = "Real person customer.",
                [nameof(TransferCustomerType.Corporate)] = "Company customer, tax number required."
            }
        };

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        var enumType = Nullable.GetUnderlyingType(context.Type) ?? context.Type;
        if (!enumType.IsEnum)
        {
            return;
        }

        schema.Type = "string";
        schema.Format = null;
        schema.Enum = Enum.GetNames(enumType)
            .Select(name => (IOpenApiAny)new OpenApiString(name))
            .ToList();

        if (EnumDescriptions.TryGetValue(enumType, out var descriptions))
        {
            var lines = descriptions.Select(x => $"{x.Key}: {x.Value}");
            schema.Description = string.IsNullOrWhiteSpace(schema.Description)
                ? string.Join(Environment.NewLine, lines)
                : $"{schema.Description}{Environment.NewLine}{string.Join(Environment.NewLine, lines)}";
        }
    }
}
