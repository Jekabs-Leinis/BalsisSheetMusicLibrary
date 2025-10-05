using BalsisNoteSheetLibrary.Server.Application.DTOs.Auth;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Services;

public class AuthService(SignInManager<IdentityUser> signInManager)
    : IAuthService
{
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto)
    {
        var user = await signInManager.UserManager.FindByNameAsync(loginDto.UserName);

        if (user == null)
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            loginDto.Password,
            true,
            false);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        var isAdmin = await signInManager.UserManager.IsInRoleAsync(user, Role.Admin);

        return new LoginResponseDto
        {
            UserName = user.UserName,
            IsAdmin = isAdmin
        };
    }

    public async Task LogoutAsync()
    {
        await signInManager.SignOutAsync();
    }

    public async Task ChangePasswordAsync(ChangePasswordRequestDto changePasswordDto)
    {
        var user = await signInManager.UserManager.FindByNameAsync(changePasswordDto.UserName);

        if (user == null)
        {
            throw new InvalidOperationException("Invalid username or password.");
        }

        var token = await signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
        var result = await signInManager.UserManager.ResetPasswordAsync(user, token, changePasswordDto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
        }
    }
}