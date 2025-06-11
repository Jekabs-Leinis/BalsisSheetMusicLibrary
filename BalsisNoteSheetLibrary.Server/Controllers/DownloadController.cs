using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class DownloadController(AppDbContext context, IWebHostEnvironment webHostEnvironment) : ControllerBase
{
    [HttpGet("{filename}")]
    public IActionResult Index(string filename)
    {
        // Basic sanitization: ensure filename doesn't contain path traversal characters.
        if (string.IsNullOrEmpty(filename) || filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
        {
            return BadRequest(new AppResponse<object>(null, false, "Invalid filename."));
        }
        
        // Additional check to ensure the filename is not a path traversal attempt.
        var sanitizedFilename = Path.GetFileName(filename);

        var sheet = context.NoteSheets.FirstOrDefault(s => s.Filename == sanitizedFilename);

        if (sheet is null)
        {
            return NotFound(new AppResponse<object>(null, false, "Note sheet not found."));
        }
        
        var path = Path.Combine(webHostEnvironment.ContentRootPath, "Static", "Sheets", sheet.GetSystemFileName());

        if (System.IO.File.Exists(path))
        {
            return PhysicalFile(path, "application/octet-stream", sanitizedFilename);
        }

        // TODO: eventually remove this fallback
        path = Path.Combine(webHostEnvironment.ContentRootPath, "Static", "Sheets", sanitizedFilename);
            
        if (!System.IO.File.Exists(path))
        {
            return NotFound(new AppResponse<object>(null, false, "File not found on server."));
        }

        return PhysicalFile(path, "application/octet-stream", sanitizedFilename);
    }
}

