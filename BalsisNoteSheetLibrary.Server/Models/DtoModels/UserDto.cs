using Microsoft.AspNetCore.Identity;

namespace BalsisNoteSheetLibrary.Server.Models.DtoModels;

public class UserDto(IdentityUser user)
{
    public string Id { get; set; } = user.Id;

    public string? Email { get; set; } = user.Email;

    // Determined by the user's role
    public bool? IsAdmin { get; set; } = false;
}