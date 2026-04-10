using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisSheetMusicLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class DownloadController(
    ISheetMusicService sheetMusicService,
    IFileStorageService fileStorageService,
    ILogger<DownloadController> logger) : ControllerBase
{
    /**
     * We don't really care about the filename, but we include it in the URL to ensure human readability.
     * The download filename should come from Content-Disposition header, but without it filename from the url is used
     * Thus we redirect to a correct value if it is changed.
     */
    [HttpGet("{id:int}/{filename}")]
    public async Task<IActionResult> Index(uint id, string filename)
    {
        var sheet = await sheetMusicService.GetSheetMusicAsync(id);

        if (sheet == null)
        {
            logger.LogWarning("User attempted to download a non-existent sheet music with ID {Id}.", id);

            return NotFound("Sheet music not found.");
        }
        
        if (!string.Equals(filename, sheet.FileName, StringComparison.OrdinalIgnoreCase))
        {
            // Redirect to the exact same action, but with the correct filename.
            // This changes the URL in the user's browser.
            return RedirectToAction(nameof(Index), new { id, filename = sheet.FileName });
        }

        if (!sheetMusicService.HasValidFile(sheet))
        {
            logger.LogError(
                "Sheet music with ID {Id} has no valid file associated with it. System filename: {filename}",
                id,
                sheet.SystemFileName
            );

            return NotFound("No valid file associated with this sheet music.");
        }

        var filePath = fileStorageService.GetSafeFilePath(sheet.SystemFileName!);
        var mimeType = GetMimeType(sheet.FileName!);
        
        
        var contentDisposition = new System.Net.Mime.ContentDisposition
        {
            FileName = sheet.FileName,
            Inline = true 
        };
        Response.Headers.Append("Content-Disposition", contentDisposition.ToString());

        return PhysicalFile(filePath, mimeType, null);
    }

    private static string GetMimeType(string filename)
    {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
    }
}