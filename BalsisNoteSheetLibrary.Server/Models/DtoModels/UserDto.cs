using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace BalsisNoteSheetLibrary.Server.Models.DtoModels
{
    public class UserDto(IdentityUser user)
    {
        public string Id { get; set; } = user.Id;
        public string? Email { get; set; } = user.Email;
        // Delermined by the user's role
        public bool? IsAdmin { get; set; } = false;
    }
}
