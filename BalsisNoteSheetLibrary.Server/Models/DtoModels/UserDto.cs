using Microsoft.AspNetCore.Identity;

namespace BalsisNoteSheetLibrary.Server.Models.DtoModels;

public class UserDto
{
    public string Id { get; set; }
    public string? Email { get; set; }
    public bool IsAdmin { get; set; }
    
    public UserDto(IdentityUser user, bool isAdmin)
    {
        Id = user.Id;
        Email = user.Email;
        IsAdmin = isAdmin;
    }
}

