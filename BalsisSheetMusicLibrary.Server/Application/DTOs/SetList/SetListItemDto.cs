using BalsisSheetMusicLibrary.Server.Application.DTOs.SheetMusic;
using BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;

public class SetListItemDto
{
    public uint? SetListId { get; set; }
    public uint? SheetMusicId { get; set; }
    public uint? Order { get; set; }
    
    public SheetMusicDto? SheetMusic { get; set; }

    public static SetListItemDto FromEntity(SetListItem entity)
    {
        return new SetListItemDto
        {
            SetListId = entity.SetListId,
            SheetMusicId = entity.SheetMusicId,
            Order = entity.Order,
            SheetMusic = entity.SheetMusic != null ? SheetMusicDto.FromEntity(entity.SheetMusic) : null
        };
    }
}