using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TransferService.API.Extensions;

public sealed class SwaggerIdempotencyHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!string.Equals(context.ApiDescription.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Idempotency-Key",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Required for transfer creation to guarantee idempotency."
        });
    }
}
