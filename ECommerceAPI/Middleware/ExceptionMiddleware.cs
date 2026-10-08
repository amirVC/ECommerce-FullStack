using System.Text.Json;
using ECommerceAPI.Exceptions;
using ECommerceAPI.Services.Interfaces;

namespace ECommerceAPI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
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
            var statusCode = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                BadRequestException => StatusCodes.Status400BadRequest,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception. Method={Method}, Path={Path}, StatusCode={StatusCode}, TraceId={TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    context.TraceIdentifier);

                await TryWriteAuditLogAsync(context, ex, statusCode);
            }
            else
            {
                _logger.LogWarning(
                    ex,
                    "Handled application exception. Method={Method}, Path={Path}, StatusCode={StatusCode}, TraceId={TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    statusCode,
                    context.TraceIdentifier);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                StatusCode = statusCode,
                Message = ex.Message
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    }

    private static async Task TryWriteAuditLogAsync(
        HttpContext context,
        Exception ex,
        int statusCode)
    {
        try
        {
            var auditLogService = context.RequestServices
                .GetRequiredService<IAuditLogService>();

            await auditLogService.LogAsync(
                action: "UnhandledException",
                entityName: null,
                entityId: null,
                details:
                    $"{ex.GetType().Name} at {context.Request.Method} {context.Request.Path} " +
                    $"(status {statusCode}, traceId {context.TraceIdentifier}): {ex.Message}");
        }
        catch
        {
        }
    }
}