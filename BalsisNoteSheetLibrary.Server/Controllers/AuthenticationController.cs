using BalsisNoteSheetLibrary.Server.DTOs;
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
    [HttpPost]
    [AllowAnonymous]
    public async Task<BaseResponseDto<LoginResponseDto>> Login(LoginRequestDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return new BaseResponseDto<LoginResponseDto>(null, false, errors);
        }

        var identityUser = await signInManager.UserManager.FindByNameAsync(loginDto.UserName);

        if (identityUser == null)
        {
            return new BaseResponseDto<LoginResponseDto>(null, false, "Invalid username or password");
        }

        var result = await signInManager.PasswordSignInAsync(
            identityUser,
            loginDto.Password,
            isPersistent: true,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return new BaseResponseDto<LoginResponseDto>(null, false, "Invalid username or password");
        }

        var isAdmin = await signInManager.UserManager.IsInRoleAsync(identityUser, Role.Admin);

        var response = new LoginResponseDto
        {
            Id = identityUser.Id,
            UserName = identityUser.UserName,
            IsAdmin = isAdmin
        };

        return new BaseResponseDto<LoginResponseDto>(response);
    }

    [HttpPost]
    public async Task<BaseResponseDto> Logout()
    {
        await signInManager.SignOutAsync();
        return new BaseResponseDto("User logged out successfully");
    }

    [HttpPost]
    public async Task<BaseResponseDto> ChangePassword(ChangePasswordRequestDto changePasswordDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return new BaseResponseDto(errors, false);
        }

        var user = await signInManager.UserManager.FindByNameAsync(changePasswordDto.UserName);

        if (user == null)
        {
            return new BaseResponseDto("User not found", false);
        }

        var resetToken = await signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
        var result = await signInManager.UserManager.ResetPasswordAsync(
            user,
            resetToken,
            changePasswordDto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new BaseResponseDto($"Failed to change password: {errors}", false);
        }

        return new BaseResponseDto("Password changed successfully");
    }
}