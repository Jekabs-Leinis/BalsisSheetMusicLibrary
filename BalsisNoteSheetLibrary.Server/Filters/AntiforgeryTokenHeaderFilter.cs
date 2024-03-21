using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BalsisNoteSheetLibrary.Server.Filters;

public class AntiforgeryTokenHeaderFilter(IAntiforgery antiforgery) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var tokens = antiforgery.GetAndStoreTokens(context.HttpContext);
        context.HttpContext.Request.Headers["X-CSRF-TOKEN"] = tokens.RequestToken;
        
        await next();
    }
}