using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthenticationController(
    SignInManager<IdentityUser> signInManager
) : ControllerBase
{
    public record UserDto(string Id, string? UserName, bool IsAdmin);

    public record LoginForm(string UserName, string Password);

    [HttpPost]
    [AllowAnonymous]
    public async Task<AppResponse<UserDto>> Login(LoginForm input)
    {
        if (!ModelState.IsValid)
        {
            return new AppResponse<UserDto>(null, false, "Invalid email or password");
        }

        var identityUser = await signInManager.UserManager.FindByNameAsync(input.UserName);

        if (identityUser == null)
        {
            return new AppResponse<UserDto>(null, false, "Invalid email or password");
        }

        var result =
            await signInManager.PasswordSignInAsync(identityUser, input.Password, true, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return new AppResponse<UserDto>(null, false, "Invalid email or password");
        }

        var isAdmin = await signInManager.UserManager.IsInRoleAsync(identityUser, Role.Admin);

        return new AppResponse<UserDto>(new UserDto(identityUser.Id, identityUser.Email, isAdmin), true);

    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return Ok(new { Success = true, Message = "User logged out successfully!" });
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(string username, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newPassword))
        {
            return BadRequest("Username and new password must be provided.");
        }

        var user = await signInManager.UserManager.FindByNameAsync(username);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var resetToken = await signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
        var result = await signInManager.UserManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (result.Succeeded)
        {
            return Ok("Password changed successfully.");
        }

        return BadRequest("Failed to change password: " + string.Join(", ", result.Errors.Select(e => e.Description)));
    }

}

