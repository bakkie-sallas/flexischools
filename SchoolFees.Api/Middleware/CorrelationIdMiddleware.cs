using System.Diagnostics;

namespace SchoolFees.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add correlation ID to response headers
        context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
        
        // Add correlation ID to logging scope
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["Method"] = context.Request.Method,
            ["Path"] = context.Request.Path.Value ?? "",
            ["QueryString"] = context.Request.QueryString.Value ?? ""
        });

        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Request started: {Method} {Path}{QueryString}", 
            context.Request.Method, 
            context.Request.Path.Value, 
            context.Request.QueryString.Value);

        try
        {
            await _next(context);
            
            stopwatch.Stop();
            _logger.LogInformation("Request completed: {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request failed: {Method} {Path} failed in {ElapsedMilliseconds}ms",
                context.Request.Method,
                context.Request.Path.Value,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeaderName].FirstOrDefault();
        
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Store in HttpContext for potential access by other components
        context.Items["CorrelationId"] = correlationId;
        
        return correlationId;
    }
}
