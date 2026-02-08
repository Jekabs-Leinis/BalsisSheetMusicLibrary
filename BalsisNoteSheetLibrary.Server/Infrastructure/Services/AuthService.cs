using BalsisNoteSheetLibrary.Server.Application.DTOs.Auth;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Services;

public class AuthService(SignInManager<IdentityUser> signInManager, IHttpContextAccessor httpContextAccessor)
    : IAuthService
{
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto)
    {
        var user = await signInManager.UserManager.FindByNameAsync(loginDto.UserName);

        SignInResult result;
        if (user != null)
        {
            result = await signInManager.PasswordSignInAsync(user, loginDto.Password, true, false);
        }
        else
        {
            // To prevent user enumeration attacks, we perform a dummy password check even if the user doesn't exist
            _ = await signInManager.PasswordSignInAsync(new IdentityUser(), loginDto.Password, true, false);
            result = SignInResult.Failed;
        }
        
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Invalid username or password");
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
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity is not { IsAuthenticated: true })
        {
            return null;
        }
        
        var currentUser = await signInManager.UserManager.GetUserAsync(httpContext.User);
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