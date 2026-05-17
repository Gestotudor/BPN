using CustomerService.Domain.Enums;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CustomerService.API.Extensions;

public sealed class CustomerEnumSchemaFilter : ISchemaFilter
{
    private static readonly IReadOnlyDictionary<Type, IReadOnlyDictionary<string, string>> EnumDescriptions =
        new Dictionary<Type, IReadOnlyDictionary<string, string>>
        {
            [typeof(CustomerType)] = new Dictionary<string, string>
            {
                [nameof(CustomerType.Individual)] = "Real person customer.",
                [nameof(CustomerType.Corporate)] = "Company customer, tax number required."
            },
            [typeof(CustomerStatus)] = new Dictionary<string, string>
            {
                [nameof(CustomerStatus.Active)] = "Customer can send and receive transfers.",
                [nameof(CustomerStatus.Passive)] = "Customer exists but cannot transact.",
                [nameof(CustomerStatus.Blocked)] = "Customer is blocked; pending transfers must be cancelled."
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
