namespace UserManagement.Middleware;

public static class GlobalExceptionMiddlewareExtension
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}