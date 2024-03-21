using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace BalsisNotis.Server.Models.DtoModels
{
    public class UserDto(User user)
    {
        public int? Id { get; set; } = user.Id;
        public string? Email { get; set; } = user.Email;
        public bool? IsAdmin { get; set; } = user.IsAdmin;
    }
}
