using BalsisNoteSheetLibrary.Server.Application.DTOs.NoteSheet;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class NoteSheetController(
    INoteSheetService noteSheetService,
    INoteSheetRenameService renameService,
    ILogger<NoteSheetController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await noteSheetService.GetAllNoteSheetsAsync();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(uint id)
    {
        var result = await noteSheetService.GetNoteSheetAsync(id);

        if (result == null)
        {
            logger.LogWarning("Tried to access non-existent note sheet with ID {Id}", id);
            
            return NotFound("Note sheet not found");
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Add([FromForm] CreateNoteSheetDto createDto, IFormFile file)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for CreateNoteSheetDto: {ModelState}", ModelState);
            
            return BadRequest(ModelState);
        }

        if (file.Length == 0)
        {
            logger.LogWarning("User attempted to upload an empty PDF file.");
            
            return BadRequest("PDF file is required");
        }

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Invalid file type for CreateNoteSheetDto: {FileType}", file.ContentType);
            
            return BadRequest("Only PDF files are allowed");
        }

        await using var stream = file.OpenReadStream();
        var result = await noteSheetService.CreateNoteSheetAsync(createDto, stream);

        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Update([FromForm] UpdateNoteSheetDto updateDto, IFormFile? file)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for UpdateNoteSheetDto: {ModelState}", ModelState);
            
            return BadRequest(ModelState);
        }

        var fileStream = file is { Length: > 0 } ? file.OpenReadStream() : null;

        try
        {
            var result = await noteSheetService.UpdateNoteSheetAsync(updateDto, fileStream);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Failed to update note sheet with ID {Id}", updateDto.Id);
            
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Delete(uint id)
    {
        try
        {
            await noteSheetService.DeleteNoteSheetAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Failed to delete note sheet with ID {Id}", id);
            
            return NotFound(ex.Message);
        }
        
        return Ok("Note sheet deleted successfully");
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> RenameAllFilenames()
    {
        await renameService.RenameAllFilenamesAsync();

        return Ok("Rename process started in the background");
    }
}