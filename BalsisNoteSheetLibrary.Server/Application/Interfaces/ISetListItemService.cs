using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;

namespace BalsisNoteSheetLibrary.Server.Application.Interfaces;

public interface ISetListItemService
{
    Task MoveSetListItemAsync(MoveSetListItemDto dto);
}