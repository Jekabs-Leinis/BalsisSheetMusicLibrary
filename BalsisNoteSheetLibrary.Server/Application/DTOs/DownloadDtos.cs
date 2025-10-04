using System.ComponentModel.DataAnnotations;

namespace BalsisNoteSheetLibrary.Server.Application.DTOs;

public class DownloadRequestDto
{
    [Required(ErrorMessage = "ID is required")]
    public uint Id { get; set; }

    [Required(ErrorMessage = "Filename is required")]
    public string Filename { get; set; } = string.Empty;
}

public class DownloadResponseDto
{
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public string? DownloadName { get; set; }
}