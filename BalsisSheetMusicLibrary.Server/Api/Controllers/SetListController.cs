using BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;
using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisSheetMusicLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class SetListController(ISetListService setListService, ILogger<SetListController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SetListDto>>> GetAll(bool withSheetMusic = false)
    {
        var setLists = await setListService.GetAllSetListsAsync(withSheetMusic);

        return Ok(setLists);
    }

    [HttpGet]
    [Authorize(Roles = Role.Admin)]
    public async Task<ActionResult<IEnumerable<SetListDto>>> GetAllArchived()
    {
        var setLists = await setListService.GetAllArchivedSetListsAsync();

        return Ok(setLists);
    }

    [HttpGet("get/{id:int}")]
    public async Task<ActionResult<SetListDto>> Get(uint id)
    {
        var setList = await setListService.GetSetListByIdAsync(id);

        if (setList == null)
        {
            logger.LogWarning("User attempted to access a non-existent set list with ID {Id}.", id);
            
            return NotFound("Set list not found.");
        }

        return Ok(setList);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<ActionResult<SetListDto>> Add([FromBody] CreateSetListDto dto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for CreateSetListDto: {ModelState}", ModelState);
            
            return BadRequest(ModelState);
        }

        var created = await setListService.CreateSetListAsync(dto);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<ActionResult<SetListDto>> Update([FromBody] UpdateSetListDto dto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for UpdateSetListDto: {ModelState}", ModelState);
            
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await setListService.UpdateSetListAsync(dto);

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Failed to update set list with ID {Id}.", dto.Id);
            
            return NotFound("Set list not found.");
        }
    }

    [HttpDelete("{setListId:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Delete(uint setListId)
    {
        await setListService.DeleteSetListAsync(setListId);

        return Ok("Set list deleted.");
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Move([FromBody] MoveSetListDto dto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for MoveSetListDto: {ModelState}", ModelState);

            return BadRequest(ModelState);
        }

        try
        {
            await setListService.MoveSetListAsync(dto);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok("Set list order updated.");
    }

    [HttpPost("{setListId:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Archive(uint setListId)
    {
        await setListService.ArchiveSetListAsync(setListId);

        return Ok("Set list archived.");
    }

    [HttpPost("{setListId:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Restore(uint setListId)
    {
        await setListService.RestoreSetListAsync(setListId);

        return Ok("Set list restored.");
    }
}