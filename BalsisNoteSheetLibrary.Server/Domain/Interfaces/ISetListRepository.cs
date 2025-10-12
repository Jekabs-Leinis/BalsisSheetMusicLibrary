using BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Server.Domain.Interfaces;

public interface ISetListRepository : IBaseRepository<SetList>
{
    new Task<SetList?> GetByIdAsync(uint id);
    new Task<List<SetList>> GetAllAsync();
    Task<List<SetList>> GetAllWithNoteSheetsAsync();
    Task<List<SetList>> GetAllArchivedAsync();
    Task<List<SetList>> GetAllWithTrackingAsync();
    Task<uint> GetMaxOrderAsync();
}