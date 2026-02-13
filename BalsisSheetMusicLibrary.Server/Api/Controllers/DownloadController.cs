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

        if (!sheetMusicService.HasValidFile(sheet))
        {
            logger.LogError(
                "Sheet music with ID {Id} has no valid file associated with it. System filename: {filename}",
                id,
                sheet.SystemFileName
            );

            return NotFound("No valid file associated with this sheet music.");
        }

        return PhysicalFile(fileStorageService.GetSafeFilePath(sheet.SystemFileName!), "application/octet-stream", sheet.FileName);
    }
}