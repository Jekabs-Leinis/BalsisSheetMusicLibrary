using BalsisNoteSheetLibrary.Server.Application.DTOs;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthenticationController(
    SignInManager<IdentityUser> signInManager
) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequestDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var identityUser = await signInManager.UserManager.FindByNameAsync(loginDto.UserName);

        if (identityUser == null)
        {
            return NotFound("Invalid username or password");
        }

        var result = await signInManager.PasswordSignInAsync(
            identityUser,
            loginDto.Password,
            true,
            false);

        if (!result.Succeeded)
        {
            return NotFound("Invalid username or password");
        }

        var isAdmin = await signInManager.UserManager.IsInRoleAsync(identityUser, Role.Admin);

        var response = new LoginResponseDto
        {
            Id = identityUser.Id,
            UserName = identityUser.UserName,
            IsAdmin = isAdmin
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();

        return Ok("User logged out successfully");
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            BadRequest(ModelState);
        }

        var user = await signInManager.UserManager.FindByNameAsync(changePasswordDto.UserName);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var resetToken = await signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
        var result = await signInManager.UserManager.ResetPasswordAsync(
            user,
            resetToken,
            changePasswordDto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Problem(
                $"Failed to change password: {errors}",
                statusCode: 500,
                title: "An internal error occurred");
        }

        return Ok("Password changed successfully");
    }
}