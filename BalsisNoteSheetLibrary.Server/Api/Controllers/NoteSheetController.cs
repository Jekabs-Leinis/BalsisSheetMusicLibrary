using BalsisNoteSheetLibrary.Server.Application.DTOs;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class NoteSheetController(INoteSheetService noteSheetService, INoteSheetRenameService renameService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await noteSheetService.GetAllNoteSheetsAsync();

        return Ok(new BaseResponseDto<IEnumerable<NoteSheetDto>>(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(uint id)
    {
        var result = await noteSheetService.GetNoteSheetAsync(id);

        if (result == null)
        {
            return NotFound(new BaseResponseDto<NoteSheetDto?>(null, false, "Note sheet not found"));
        }

        return Ok(new BaseResponseDto<NoteSheetDto?>(result));
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Add([FromForm] CreateNoteSheetDto createDto, IFormFile file)
    {
        if (file.Length == 0)
        {
            return BadRequest(new BaseResponseDto<NoteSheetDto>(null, false, "PDF file is required"));
        }

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new BaseResponseDto<NoteSheetDto>(null, false, "Only PDF files are allowed"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

            return BadRequest(new BaseResponseDto<NoteSheetDto>(null, false, string.Join(", ", errors)));
        }

        await using var stream = file.OpenReadStream();
        var result = await noteSheetService.CreateNoteSheetAsync(createDto, stream);

        return CreatedAtAction(nameof(Get), new { id = result.Id }, new BaseResponseDto<NoteSheetDto>(result));
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Update([FromForm] UpdateNoteSheetDto updateDto, IFormFile? file)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

            return BadRequest(new BaseResponseDto<NoteSheetDto>(null, false, string.Join(", ", errors)));
        }

        var fileStream = file is { Length: > 0 } ? file.OpenReadStream() : null;

        try
        {
            var result = await noteSheetService.UpdateNoteSheetAsync(updateDto, fileStream);

            return Ok(new BaseResponseDto<NoteSheetDto>(result));
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new BaseResponseDto<NoteSheetDto>(null, false, ex.Message));
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Delete(uint id)
    {
        await noteSheetService.DeleteNoteSheetAsync(id);

        return Ok(new BaseResponseDto("Note sheet deleted successfully"));
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> RenameAllFilenames()
    {
        await renameService.RenameAllFilenamesAsync();

        return Ok(new BaseResponseDto("Rename process started in the background"));
    }
}