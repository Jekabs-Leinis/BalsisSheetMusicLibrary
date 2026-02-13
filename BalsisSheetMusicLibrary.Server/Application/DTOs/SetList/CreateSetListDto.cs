using System.ComponentModel.DataAnnotations;
using BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;

public class CreateSetListDto
{
    [Required] public required string Title { get; set; }

    public Domain.Entities.SetList ToEntity()
    {
        return new Domain.Entities.SetList
        {
            Title = Title
        };
    }
}

public class CreateSetListItemDto
{
    public uint NoteSheetId { get; set; }
    public uint Order { get; set; }

    public SetListItem ToEntity()
    {
        return new SetListItem
        {
            NoteSheetId = NoteSheetId,
            Order = Order
        };
    }
}