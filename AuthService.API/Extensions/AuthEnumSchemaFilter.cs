using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AuthService.API.Extensions;

public sealed class AuthEnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
        {
            return;
        }

        // The Auth API does not currently expose public enums in request or response DTOs.
        // This filter keeps the configuration consistent with the other services.
        schema.Description ??= "String enum values are returned and accepted by this API.";
    }
}
