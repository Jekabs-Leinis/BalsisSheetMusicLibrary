using System.ComponentModel.DataAnnotations;
using BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;

public class UpdateSetListDto
{
    [Required] public uint Id { get; set; }

    [Required] public string Title { get; set; }

    public List<UpdateSetListItemDto> Items { get; set; }

    public Domain.Entities.SetList ToEntity()
    {
        return new Domain.Entities.SetList
        {
            Id = Id,
            Title = Title,
            Items = Items.Select(i => i.ToEntity()).ToList()
        };
    }
}

public class UpdateSetListItemDto
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