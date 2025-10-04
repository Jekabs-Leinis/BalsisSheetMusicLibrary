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
    [HttpGet(Name = "GetAll")]
    public async Task<ActionResult<IEnumerable<SetListDto>>> GetAll()
    {
        var setLists = await setListService.GetAllSetListsAsync();

        return Ok(setLists);
    }

    [HttpGet("{id:int}")]
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

    [HttpPut]
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
        catch
        {
            //TODO LOG
            return Problem();
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Delete(uint id)
    {
        await setListService.DeleteSetListAsync(id);

        return Ok("Set list deleted.");
    }

    [HttpPost("{id:int}/order")]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> UpdateOrder(uint id, [FromBody] uint newOrder)
    {
        await setListService.UpdateSetListOrderAsync(id, newOrder);

        return Ok("Set list order updated.");
    }
}