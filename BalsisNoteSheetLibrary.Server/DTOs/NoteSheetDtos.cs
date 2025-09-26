using BalsisNoteSheetLibrary.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace BalsisNoteSheetLibrary.Server.DTOs;

public class NoteSheetDto
{
    public uint? Id { get; set; }
    public string? Title { get; set; } = string.Empty;
    public string? Author { get; set; } = string.Empty;
    public string? Lyricist { get; set; } = string.Empty;
    public uint? Year { get; set; }
    public bool IsLatvian { get; set; }
    public string? Filename { get; set; }
    public string? SystemFileName { get; set; }

    public static NoteSheetDto FromEntity(NoteSheet entity)
    {
        return new NoteSheetDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Author = entity.Author,
            Lyricist = entity.Lyricist,
            Year = entity.Year,
            IsLatvian = entity.IsLatvian,
            Filename = entity.Filename,
            SystemFileName = entity.SystemFileName
        };
    }
}

public class CreateNoteSheetDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
    public string? Title { get; set; }

    [StringLength(100, ErrorMessage = "Author cannot be longer than 100 characters")]
    public string? Author { get; set; }

    [StringLength(100, ErrorMessage = "Lyricist cannot be longer than 100 characters")]
    public string? Lyricist { get; set; }

    [Range(1000, 2100, ErrorMessage = "Year must be between 1000 and 2100")]
    public uint? Year { get; set; }

    public bool IsLatvian { get; set; }

    public NoteSheet ToEntity()
    {
        return new NoteSheet
        {
            Title = Title,
            Author = Author,
            Lyricist = Lyricist,
            Year = Year,
            IsLatvian = IsLatvian
        };
    }
}

public class UpdateNoteSheetDto
{
    [Required(ErrorMessage = "Id is required")]
    public uint Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
    public string? Title { get; set; }

    [StringLength(100, ErrorMessage = "Author cannot be longer than 100 characters")]
    public string? Author { get; set; }

    [StringLength(100, ErrorMessage = "Lyricist cannot be longer than 100 characters")]
    public string? Lyricist { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Year must be between 0 and 2147483647")]
    public uint? Year { get; set; }

    public bool IsLatvian { get; set; }

    public void UpdateEntity(NoteSheet entity)
    {
        entity.Title = Title ?? string.Empty;
        entity.Author = Author;
        entity.Lyricist = Lyricist;
        entity.Year = Year;
        entity.IsLatvian = IsLatvian;
    }
}
