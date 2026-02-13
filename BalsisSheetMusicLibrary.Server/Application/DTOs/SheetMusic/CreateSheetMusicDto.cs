using System.ComponentModel.DataAnnotations;

namespace BalsisSheetMusicLibrary.Server.Application.DTOs.SheetMusic;

public class CreateSheetMusicDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
    public string? Title { get; set; }

    [StringLength(200, ErrorMessage = "Author cannot be longer than 200 characters")]
    public string? Author { get; set; }

    [StringLength(200, ErrorMessage = "Lyricist cannot be longer than 200 characters")]
    public string? Lyricist { get; set; }

    public uint? Year { get; set; }
    public bool IsLatvian { get; set; }

    public Domain.Entities.SheetMusic ToEntity()
    {
        return new Domain.Entities.SheetMusic
        {
            Title = Title,
            Author = Author,
            Lyricist = Lyricist,
            Year = Year,
            IsLatvian = IsLatvian
        };
    }
}