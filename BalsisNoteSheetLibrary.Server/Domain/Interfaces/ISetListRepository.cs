using BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Server.Domain.Interfaces;

public interface ISetListRepository : IBaseRepository<SetList>
{
    Task<uint> GetMaxOrderAsync();
    Task<SetList?> GetByIdWithItemsAsync(uint setListId);
}