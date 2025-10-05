using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class DownloadController(INoteSheetService noteSheetService) : ControllerBase
{
    /**
     * We don't really care about the filename, but we include it in the URL to ensure readability.
     */
    [HttpGet("{id:int}/{filename}")]
    public async Task<IActionResult> Index(uint id, string filename)
    {
        var sheet = await noteSheetService.GetNoteSheetAsync(id);

        if (sheet == null)
        {
            return NotFound("Note sheet not found.");
        }

        if (!noteSheetService.HasValidFile(sheet))
        {
            return NotFound("No valid file associated with this note sheet.");
        }

        return PhysicalFile(sheet.SystemFileName, "application/octet-stream", sheet.FileName);
    }
}