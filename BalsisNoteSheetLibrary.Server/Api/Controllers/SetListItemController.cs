using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class SetListItemController(ISetListItemService setListItemService): ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Move([FromBody] MoveSetListItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await setListItemService.MoveSetListItemAsync(dto);

        return Ok("Set list item order updated.");
    }
}