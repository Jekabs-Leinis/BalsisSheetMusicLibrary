using BalsisNoteSheetLibrary.Server.Application.DTOs;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class DownloadController(AppDbContext context, IWebHostEnvironment webHostEnvironment) : ControllerBase
{
    /**
     * We don't really care about the filename, but we accept it to ensure readability of the URL.
     */
    [HttpGet("{id:int}/{filename}")]
    public async Task<IActionResult> Index(uint id, string filename)
    {

        var sheet = await context.NoteSheets
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sheet is null)
        {
            return NotFound(new BaseResponseDto("Note sheet not found.", false));
        }

        // Try with SystemFileName first (new format)
        var path = Path.Combine(webHostEnvironment.ContentRootPath, "Static", "Sheets", sheet.SystemFileName ?? string.Empty);
        if (System.IO.File.Exists(path))
        {
            return PhysicalFile(path, "application/octet-stream", sheet.Filename);
        }

        // Fallback to Filename (legacy support)
        path = Path.Combine(webHostEnvironment.ContentRootPath, "Static", "Sheets", sheet.Filename ?? string.Empty);
        if (!System.IO.File.Exists(path))
        {
            return NotFound(new BaseResponseDto("File not found on server.", false));
        }

        return PhysicalFile(path, "application/octet-stream", sheet.Filename);
    }
}
