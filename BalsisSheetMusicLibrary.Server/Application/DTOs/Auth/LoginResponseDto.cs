namespace BalsisSheetMusicLibrary.Server.Application.DTOs.Auth;

public class LoginResponseDto
{
    public string? UserName { get; set; }
    public bool IsAdmin { get; set; }
}