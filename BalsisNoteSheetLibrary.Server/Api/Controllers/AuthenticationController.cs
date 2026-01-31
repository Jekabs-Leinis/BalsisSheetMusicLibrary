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

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCurrentUser()
    {
        return await authService.GetCurrentUserAsync() is { } user
            ? Ok(user)
            : Unauthorized("No user is currently logged in.");
    }

    [HttpPost]
    [Authorize(Roles = $"{Role.Admin}")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (Environment.GetEnvironmentVariable("LIB_ALLOW_MANUAL_PASSWORD_RESET") != "1")
        {
            return BadRequest("Manuāla paroles nomaiņa ir izslēgta.");
        }

        try
        {
            await authService.ChangePasswordAsync(changePasswordDto);

            return Ok("Password changed successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest($"Failed to change password: {ex.Message}");
        }
    }
}