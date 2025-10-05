using System.ComponentModel.DataAnnotations;

namespace BalsisNoteSheetLibrary.Server.Application.DTOs.Auth;

public class ChangePasswordRequestDto
{
    [Required(ErrorMessage = "Username is required")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
        MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;
}