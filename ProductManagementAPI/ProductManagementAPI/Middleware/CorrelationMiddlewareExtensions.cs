using Microsoft.AspNetCore.Builder;

namespace ProductManagementAPI.Middleware;

public static class CorrelationMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationMiddleware>();
    }
}