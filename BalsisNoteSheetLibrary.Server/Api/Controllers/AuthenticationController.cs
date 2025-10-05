using BalsisNoteSheetLibrary.Server.Application.DTOs.Auth;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthenticationController(IAuthService authService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequestDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await authService.LoginAsync(loginDto);

            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return Unauthorized("Invalid username or password.");
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await authService.LogoutAsync();

        return Ok("User logged out successfully");
    }

    [HttpPost]
    [Authorize(Roles = $"{Role.Admin}")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto changePasswordDto)
    {
        //TODO: figure out the implementation for this
        return Ok("Not implemented yet");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await authService.ChangePasswordAsync(changePasswordDto);

            return Ok("Password changed successfully.");
        }
        catch (InvalidOperationException)
        {
            return BadRequest("Failed to change password.");
        }
    }
}