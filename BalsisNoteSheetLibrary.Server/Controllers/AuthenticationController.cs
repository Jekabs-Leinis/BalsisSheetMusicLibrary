using BalsisNoteSheetLibrary.Server.Models;
using BalsisNoteSheetLibrary.Server.Models.DtoModels;
using BalsisNoteSheetLibrary.Server.Models.FormModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("Api/[controller]/[action]")]
public class AuthenticationController(
    AppDbContext context,
    SignInManager<IdentityUser> signInManager,
    IHostEnvironment env) : AppControllerBase(context)
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<AppResponse<UserDto>> Login(LoginForm input)
    {
        if (!ModelState.IsValid)
        {
            return new AppResponse<UserDto>(null, false, "Invalid email or password");
        }

        var result = await signInManager.PasswordSignInAsync(input.Email, input.Password, false, false);

        if (!result.Succeeded)
        {
            return new AppResponse<UserDto>(null, false, "Invalid email or password");
        }

        var identityUser = await signInManager.UserManager.FindByEmailAsync(input.Email);

        if (identityUser != null)
        {
            return new AppResponse<UserDto>(new UserDto(identityUser), true);
        }

        return new AppResponse<UserDto>(null, false, "Invalid email or password");
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(string email, string password)
    {
        if (!env.IsDevelopment())
        {
            return BadRequest(new { Message = "Registration is disabled in production environment" });
        }

        var result =
            await signInManager.UserManager.CreateAsync(
                new IdentityUser { UserName = email, Email = email },
                password
            );

        if (result.Succeeded)
        {
            return Ok(new { Message = "User registered successfully!" });
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

        return BadRequest(ModelState);
    }
}