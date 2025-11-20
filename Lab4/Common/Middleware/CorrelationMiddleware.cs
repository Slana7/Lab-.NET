namespace Lab4.Common.Middleware;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or generate correlation ID
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N")[..16].ToUpper();

        // Add to response headers
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // Add to logging scope
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            _logger.LogInformation("Request started - Method: {Method}, Path: {Path}, CorrelationId: {CorrelationId}",
                context.Request.Method, context.Request.Path, correlationId);

            await _next(context);

            _logger.LogInformation("Request completed - Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, CorrelationId: {CorrelationId}",
                context.Request.Method, context.Request.Path, context.Response.StatusCode, correlationId);
        }
    }
}


