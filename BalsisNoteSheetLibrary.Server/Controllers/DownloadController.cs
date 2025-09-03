using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class DownloadController(AppDbContext context, IWebHostEnvironment webHostEnvironment) : ControllerBase
{
    [HttpGet("{id:int}/{filename}")]
    public async Task<IActionResult> Index(uint id, string filename)
    {
        // We don't really care about the filename, but we accept it to ensure readability of the URL.
        var sheet = await context.NoteSheets.FindAsync(id);

        if (sheet is null)
        {
            return NotFound(new AppResponse<object>(null, false, "Note sheet not found."));
        }
        
        var path = Path.Combine(webHostEnvironment.ContentRootPath, "Static", "Sheets", sheet.GetSystemFileName());

        if (System.IO.File.Exists(path))
        {
            return PhysicalFile(path, "application/octet-stream", sheet.Filename);
        }

        // TODO: eventually remove this fallback
        path = Path.Combine(webHostEnvironment.ContentRootPath, "Static", "Sheets", sheet.Filename ?? string.Empty);
            
        if (!System.IO.File.Exists(path))
        {
            return NotFound(new AppResponse<object>(null, false, "File not found on server."));
        }

        return PhysicalFile(path, "application/octet-stream", sheet.Filename);
    }
}

