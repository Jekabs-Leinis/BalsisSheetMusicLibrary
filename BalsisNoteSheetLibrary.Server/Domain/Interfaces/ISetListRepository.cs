using BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Server.Domain.Interfaces;

public interface ISetListRepository
{
    Task<SetList?> GetByIdAsync(uint id);
    Task<IEnumerable<SetList>> GetAllAsync();
    Task<IEnumerable<SetList>> GetAllArchivedAsync();
}