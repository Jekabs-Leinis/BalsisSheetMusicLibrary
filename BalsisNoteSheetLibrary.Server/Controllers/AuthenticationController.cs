using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class AuthenticationController(
    SignInManager<IdentityUser> signInManager,
    IHostEnvironment env
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

        var result = await signInManager.PasswordSignInAsync(identityUser, input.Password, true, lockoutOnFailure: false);
        
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
}

