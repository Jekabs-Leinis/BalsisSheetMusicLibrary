using BalsisNoteSheetLibrary.Server.Application.DTOs.Auth;

namespace BalsisNoteSheetLibrary.Server.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);
    Task LogoutAsync();
    Task<CurrentUserDto?> GetCurrentUserAsync();
    Task ChangePasswordAsync(ChangePasswordRequestDto changePasswordDto);
}