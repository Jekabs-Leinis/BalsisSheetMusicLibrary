using BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Server.Domain.Interfaces;

public interface ISetListRepository : IBaseRepository<SetList>
{
    Task<uint> GetMaxOrderAsync();
    Task<SetList?> GetByIdWithItemsAsync(uint setListId);
}