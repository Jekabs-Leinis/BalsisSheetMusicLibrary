using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BalsisNotis.Server.Models
{
    public class User
    {
        public int? Id { get; set; }
        [Required(ErrorMessage = "Email cannot be empty")]
        public string? Email { get; set; }
        [NotMapped]
        [Required(ErrorMessage = "Password cannot be empty")]
        public string? Password { get; set; }
        [Column("password_hash")]
        public string? PasswordHash { get; set; }
        [Column("is_admin")]
        public bool? IsAdmin { get; set; } = false;
    }
}
