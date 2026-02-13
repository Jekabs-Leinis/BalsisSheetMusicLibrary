using BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;
using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BalsisSheetMusicLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class SetListItemController(ISetListItemService setListItemService, ILogger<SetListItemController> logger)
    : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> Move([FromBody] MoveSetListItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            logger.LogWarning("Invalid model state for MoveSetListItemDto: {ModelState}", ModelState);
            
            return BadRequest(ModelState);
        }

        try
        {
            await setListItemService.MoveSetListItemAsync(dto);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Error moving set list item with set list ID {sheetId} and note sheet {noteId}", dto.SetListId, dto.SheetMusicId);
            
            return BadRequest(ex.Message);
        }
        
        return Ok("Set list item order updated.");
    }
}