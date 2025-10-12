using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AntiforgeryController(IAntiforgery antiforgery) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public IActionResult Token()
    {
        return Ok();
    }
}