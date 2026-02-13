using Microsoft.AspNetCore.Identity;

namespace BalsisSheetMusicLibrary.Server.Application.DTOs.User;

public class UserDto
{
    public string? UserName { get; set; }

    public static UserDto FromEntity(IdentityUser user)
    {
        return new UserDto { UserName = user.UserName };
    }
}