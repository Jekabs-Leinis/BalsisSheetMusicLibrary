using Microsoft.AspNetCore.Identity;

namespace BalsisSheetMusicLibrary.Server.Application.DTOs.Auth;

public class CurrentUserDto(string? userName, bool isAdmin = false)
{
    public string? UserName { get; set; } = userName;
    public bool IsAdmin { get; set; } = isAdmin;
}