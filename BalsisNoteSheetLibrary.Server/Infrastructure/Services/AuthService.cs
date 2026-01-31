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

    public async Task<CurrentUserDto?> GetCurrentUserAsync()
    {
        if (!signInManager.IsSignedIn(signInManager.Context.User))
        {
            return null;
        }

        var currentUser = await signInManager.UserManager.GetUserAsync(signInManager.Context.User);
            
        if (currentUser == null)
        {
            return null;
        }
            
        var isAdmin = await signInManager.UserManager.IsInRoleAsync(currentUser, Role.Admin);
            
        return new CurrentUserDto(currentUser.UserName, isAdmin);

    }

    public async Task ChangePasswordAsync(ChangePasswordRequestDto changePasswordDto)
    {
        var user = await signInManager.UserManager.FindByNameAsync(changePasswordDto.UserName);

        if (user == null)
        {
            throw new InvalidOperationException("Invalid username");
        }

        var token = await signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
        var result = await signInManager.UserManager.ResetPasswordAsync(user, token, changePasswordDto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            
            throw new InvalidOperationException($"Failed to change password: {string.Join(", ", errors)}");
        }
    }
}