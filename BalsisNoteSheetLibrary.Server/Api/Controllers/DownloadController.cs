using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class DownloadController(
    INoteSheetService noteSheetService,
    IFileStorageService fileStorageService,
    ILogger<DownloadController> logger) : ControllerBase
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
            logger.LogWarning("User attempted to download a non-existent note sheet with ID {Id}.", id);

            return NotFound("Note sheet not found.");
        }

        if (!noteSheetService.HasValidFile(sheet))
        {
            logger.LogError(
                "Note sheet with ID {Id} has no valid file associated with it. System filename: {filename}",
                id,
                sheet.SystemFileName
            );

            return NotFound("No valid file associated with this note sheet.");
        }

        return PhysicalFile(fileStorageService.GetFilePath(sheet.SystemFileName!), "application/octet-stream", sheet.FileName);
    }
}