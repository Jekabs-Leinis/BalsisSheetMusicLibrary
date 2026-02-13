using BalsisSheetMusicLibrary.Server.Application.DTOs.SheetMusic;
using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisSheetMusicLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class SheetMusicController(
    ISheetMusicService sheetMusicService,
    ISheetMusicRenameService sheetRenameService,
    ILogger<SheetMusicController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await sheetMusicService.GetAllSheetMusicAsync();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(uint id)
    {
        var result = await sheetMusicService.GetSheetMusicAsync(id);

        if (result == null)
        {
            logger.LogWarning("Tried to access non-existent sheet music with ID {Id}", id);
            
            return NotFound("Sheet music not found");
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Add([FromForm] CreateSheetMusicDto createMusicDto, IFormFile file)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for CreateSheetMusicDto: {ModelState}", ModelState);
            
            return BadRequest(ModelState);
        }

        if (file.Length == 0)
        {
            logger.LogWarning("User attempted to upload an empty PDF file.");
            
            return BadRequest("PDF file is required");
        }

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Invalid file type for CreateSheetMusicDto: {FileType}", file.ContentType);
            
            return BadRequest("Only PDF files are allowed");
        }

        await using var stream = file.OpenReadStream();
        var result = await sheetMusicService.CreateSheetMusicAsync(createMusicDto, stream);

        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Update([FromForm] UpdateSheetMusicDto updateMusicDto, IFormFile? file)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for UpdateSheetMusicDto: {ModelState}", ModelState);
            
            return BadRequest(ModelState);
        }

        var fileStream = file is { Length: > 0 } ? file.OpenReadStream() : null;

        try
        {
            var result = await sheetMusicService.UpdateSheetMusicAsync(updateMusicDto, fileStream);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Failed to update sheet music with ID {Id}", updateMusicDto.Id);
            
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Delete(uint id)
    {
        try
        {
            await sheetMusicService.DeleteSheetMusicAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Failed to delete sheet music with ID {Id}", id);
            
            return NotFound(ex.Message);
        }
        
        return Ok("Sheet music deleted successfully");
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> RenameAllFilenames()
    {
        await sheetRenameService.RenameAllFilenamesAsync();

        return Ok("Rename process started in the background");
    }
}