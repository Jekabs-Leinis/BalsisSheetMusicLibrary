using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class CsrfController(IAntiforgery antiforgery) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public IActionResult GetToken()
    {
        var tokenSet = antiforgery.GetAndStoreTokens(HttpContext);
        HttpContext.Response.Cookies.Append("CSRF-TOKEN", tokenSet.RequestToken!,
            new CookieOptions { HttpOnly = false });
        return Ok();
    }
}