using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductManagementAPI.Middleware;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;
    public const string HeaderName = "X-Correlation-ID";

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.ContainsKey(HeaderName)
            ? context.Request.Headers[HeaderName].ToString()
            : Guid.NewGuid().ToString("N");
        
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(HeaderName))
                context.Response.Headers.Add(HeaderName, correlationId);
            return Task.CompletedTask;
        });

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            context.Items[HeaderName] = correlationId;
            await _next(context);
        }
    }
}