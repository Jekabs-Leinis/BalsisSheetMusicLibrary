using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class SetListController(ISetListService setListService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SetListDto>>> GetAll(bool withNoteSheets = false)
    {
        var setLists = await setListService.GetAllSetListsAsync(withNoteSheets);

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
            return BadRequest(ModelState);
        }

        try
        {
            var updated = await setListService.UpdateSetListAsync(dto);

            return Ok(updated);
        }
        catch (InvalidOperationException)
        {
            return NotFound("Set list not found.");
        }
    }

    [HttpDelete]
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
            return BadRequest(ModelState);
        }

        await setListService.MoveSetListAsync(dto);

        return Ok("Set list order updated.");
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Archive(uint id)
    {
        await setListService.ArchiveSetListAsync(id);

        return Ok("Set list archived.");
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Restore(uint id)
    {
        await setListService.RestoreSetListAsync(id);

        return Ok("Set list restored.");
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> MoveSetListItem([FromBody] MoveSetListItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await setListService.MoveSetListItemAsync(dto);

        return Ok("Set list item order updated.");
    }
}