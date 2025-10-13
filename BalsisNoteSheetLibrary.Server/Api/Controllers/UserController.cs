using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllUsersAsync();
        
        return Ok(users);
    }
}