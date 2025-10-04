using BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;

public class SetListItemDto
{
    public uint? SetListId { get; set; }
    public uint? NoteSheetId { get; set; }
    public uint? Order { get; set; }

    public static SetListItemDto FromEntity(SetListItem entity)
    {
        return new SetListItemDto
        {
            SetListId = entity.SetListId,
            NoteSheetId = entity.NoteSheetId,
            Order = entity.Order
        };
    }
}