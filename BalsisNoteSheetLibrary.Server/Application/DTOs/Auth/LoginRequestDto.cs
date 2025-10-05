using System.ComponentModel.DataAnnotations;

namespace BalsisNoteSheetLibrary.Server.Application.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;
}