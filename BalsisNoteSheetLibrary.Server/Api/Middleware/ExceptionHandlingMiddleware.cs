using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Api.Middleware;

/// <summary>
/// Middleware to catch exceptions, log them, and provide a standardized, safe error response.
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception has occurred. TraceId: {TraceId}", context.TraceIdentifier);
            
            await HandleExceptionAsync(context, ex);
        }
    }
    private static async Task  HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden, "Access denied"),
            KeyNotFoundException or FileNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid argument"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Database conflict occurred"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected internal server error has occurred")
        };
        
        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Message = message,
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        
        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
        
        await context.Response.WriteAsync(jsonResponse);
    }
}


public class ErrorResponse
{
    public string TraceId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
