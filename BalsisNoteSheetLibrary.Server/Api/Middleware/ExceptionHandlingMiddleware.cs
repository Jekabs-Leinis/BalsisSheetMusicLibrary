using System.Net;
using System.Text.Json;

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
            
            await HandleExceptionAsync(context);
        }
    }
    private static async Task  HandleExceptionAsync(HttpContext context)
    {
        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier,
            Message = "An unexpected internal server error has occurred. Please try again later.",
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        
        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        
        await context.Response.WriteAsync(jsonResponse);
    }
}


public class ErrorResponse
{
    public string TraceId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
