using System.ComponentModel.DataAnnotations;
using BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;

public class UpdateSetListDto
{
    [Required] public uint Id { get; set; }

    [Required] public required string Title { get; set; }

    public required List<UpdateSetListItemDto> Items { get; set; }

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
    public uint SheetMusicId { get; set; }
    public uint Order { get; set; }

    public SetListItem ToEntity()
    {
        return new SetListItem
        {
            SheetMusicId = SheetMusicId,
            Order = Order
        };
    }
}