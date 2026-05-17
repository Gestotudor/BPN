using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Middleware
{
    public static class ApiKeyAuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
        }
    }
}
