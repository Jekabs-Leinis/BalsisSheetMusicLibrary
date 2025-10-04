using BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Server.Application.DTOs;

public class NoteSheetDto
{
    public uint? Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Lyricist { get; set; }
    public uint? Year { get; set; }
    public string? Filename { get; set; }
    public string? SystemFileName { get; set; }
    public bool IsLatvian { get; set; }

    public static NoteSheetDto FromEntity(NoteSheet entity)
    {
        return new NoteSheetDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Author = entity.Author,
            Lyricist = entity.Lyricist,
            Year = entity.Year,
            Filename = entity.FileName,
            SystemFileName = entity.SystemFileName,
            IsLatvian = entity.IsLatvian
        };
    }
}