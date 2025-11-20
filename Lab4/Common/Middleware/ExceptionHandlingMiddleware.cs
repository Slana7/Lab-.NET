using System.Net;
using System.Text.Json;
using Lab4.Exceptions;

namespace Lab4.Common.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorResponse = exception switch
        {
            ValidationException validationEx => new ErrorResponse(
                validationEx.ErrorCode,
                validationEx.Message,
                validationEx.Errors)
            {
                TraceId = context.TraceIdentifier
            },
            
            BookNotFoundException notFoundEx => new ErrorResponse(
                notFoundEx.ErrorCode,
                notFoundEx.Message)
            {
                TraceId = context.TraceIdentifier
            },
            
            BookAlreadyExistsException existsEx => new ErrorResponse(
                existsEx.ErrorCode,
                existsEx.Message)
            {
                TraceId = context.TraceIdentifier
            },
            
            InsufficientStockException stockEx => new ErrorResponse(
                stockEx.ErrorCode,
                stockEx.Message)
            {
                TraceId = context.TraceIdentifier
            },
            
            DatabaseException dbEx => new ErrorResponse(
                dbEx.ErrorCode,
                dbEx.Message)
            {
                TraceId = context.TraceIdentifier
            },
            
            BaseException baseEx => new ErrorResponse(
                baseEx.ErrorCode,
                baseEx.Message)
            {
                TraceId = context.TraceIdentifier
            },
            
            _ => new ErrorResponse(
                "INTERNAL_SERVER_ERROR",
                "An unexpected error occurred. Please try again later.")
            {
                TraceId = context.TraceIdentifier
            }
        };

        var statusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            BookNotFoundException => (int)HttpStatusCode.NotFound,
            BookAlreadyExistsException => (int)HttpStatusCode.Conflict,
            InsufficientStockException => (int)HttpStatusCode.BadRequest,
            DatabaseException => (int)HttpStatusCode.InternalServerError,
            BaseException baseEx => baseEx.StatusCode,
            _ => (int)HttpStatusCode.InternalServerError
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}


