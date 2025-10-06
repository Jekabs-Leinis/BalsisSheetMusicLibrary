using BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Server.Domain.Interfaces;

public interface ISetListRepository
{
    Task<SetList?> GetByIdAsync(uint id);
    Task<List<SetList>> GetAllAsync();
    Task<List<SetList>> GetAllWithNoteSheetsAsync();
    Task<List<SetList>> GetAllArchivedAsync();
    Task<List<SetList>> GetAllWithTrackingAsync();
}