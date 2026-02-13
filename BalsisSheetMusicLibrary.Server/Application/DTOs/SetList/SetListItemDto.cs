using BalsisSheetMusicLibrary.Server.Application.DTOs.NoteSheet;
using BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;

public class SetListItemDto
{
    public uint? SetListId { get; set; }
    public uint? NoteSheetId { get; set; }
    public uint? Order { get; set; }
    
    public NoteSheetDto? NoteSheet { get; set; }

    public static SetListItemDto FromEntity(SetListItem entity)
    {
        return new SetListItemDto
        {
            SetListId = entity.SetListId,
            NoteSheetId = entity.NoteSheetId,
            Order = entity.Order,
            NoteSheet = entity.NoteSheet != null ? NoteSheetDto.FromEntity(entity.NoteSheet) : null
        };
    }
}