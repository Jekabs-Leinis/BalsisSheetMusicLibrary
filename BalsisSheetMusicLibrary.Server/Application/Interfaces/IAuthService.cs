using BalsisSheetMusicLibrary.Server.Application.DTOs.Auth;

namespace BalsisSheetMusicLibrary.Server.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto);
    Task LogoutAsync();
    Task<CurrentUserDto?> GetCurrentUserAsync();
    Task ChangePasswordAsync(ChangePasswordRequestDto changePasswordDto);
}