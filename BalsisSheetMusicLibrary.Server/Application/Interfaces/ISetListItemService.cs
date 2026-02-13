using BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;

namespace BalsisSheetMusicLibrary.Server.Application.Interfaces;

public interface ISetListItemService
{
    Task MoveSetListItemAsync(MoveSetListItemDto dto);
}