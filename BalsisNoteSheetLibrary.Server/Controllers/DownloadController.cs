using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController(AppDbContext context) : AppControllerBase(context)
{
    [HttpGet("{filename}")]
    public IActionResult Index(string filename)
    {
        var sheet = Context.NoteSheets.FirstOrDefault(sheet => sheet.Filename == filename);

        if (sheet is null)
        {
            return BadRequest(new AppResponse<string>(null, false, "Note sheet not found"));
        }

        var path = Path.Combine(Directory.GetCurrentDirectory(), "Static", "Sheets", filename);

        if (!System.IO.File.Exists(path))
        {
            return BadRequest(new AppResponse<string>(null, false, "File not found"));
        }

        return PhysicalFile(path, "application/octet-stream", filename);
    }
}