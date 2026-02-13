namespace BalsisSheetMusicLibrary.Server.Api.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using (logger.BeginScope("TraceId: {TraceId}", context.TraceIdentifier))
        {
            await next(context);
        }
    }
}