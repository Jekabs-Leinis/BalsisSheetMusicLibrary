using BalsisNoteSheetLibrary.Server.Models;

namespace BalsisNoteSheetLibrary.Server.DTOs;

public class SetListDto
{
    public uint Id { get; set; }
    public string? Title { get; set; } = string.Empty;
    public uint? Order { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    
    public List<SetListItemDto> Items { get; set; } = [];

    public static SetListDto FromEntity(SetList entity, bool includeNoteSheets = false)
    {
        var dto = new SetListDto
        {
            Id = entity.Id,
            Title = entity.Title,
            Order = entity.Order,
            Items = entity.Items
                .OrderBy(i => i.Order)
                .Select(i => SetListItemDto.FromEntity(i, includeNoteSheets))
                .ToList(),
            CreatedAt = entity.CreatedAt,
            ArchivedAt = entity.ArchivedAt
        };

        return dto;
    }
}

public class SetListItemDto
{
    public uint? SetListId { get; set; }
    public uint? NoteSheetId { get; set; }
    public uint? Order { get; set; }
    public NoteSheetDto? NoteSheet { get; set; }

    public static SetListItemDto FromEntity(SetListItem entity, bool includeNoteSheet = false)
    {
        var dto = new SetListItemDto
        {
            SetListId = entity.SetListId,
            NoteSheetId = entity.NoteSheetId,
            Order = entity.Order,
        };

        if (includeNoteSheet && entity.NoteSheet != null)
        {
            dto.NoteSheet = NoteSheetDto.FromEntity(entity.NoteSheet);
        }

        return dto;
    }
}

public class NoteSheetDto
{
    public uint Id { get; set; }
    public string? Title { get; set; } = string.Empty;
    public string? Author { get; set; } = string.Empty;
    public string? Lyricist { get; set; } = string.Empty;
    public uint? Year { get; set; }
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
            IsLatvian = entity.IsLatvian,
        };
    }
}
