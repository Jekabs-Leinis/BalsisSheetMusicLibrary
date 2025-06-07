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
        
        // It's often better to use the actual filename part if the input could be a path.
        var sanitizedFilename = Path.GetFileName(filename);

        var sheet = context.NoteSheets.FirstOrDefault(s => s.Filename == sanitizedFilename);

        if (sheet is null)
        {
            return NotFound(new AppResponse<object>(null, false, "Note sheet not found."));
        }
        
        var path = Path.Combine(webHostEnvironment.ContentRootPath, "Static", "Sheets", sanitizedFilename);

        if (!System.IO.File.Exists(path))
        {
            return NotFound(new AppResponse<object>(null, false, "File not found on server."));
        }

        return PhysicalFile(path, "application/octet-stream", sanitizedFilename);
    }
}

