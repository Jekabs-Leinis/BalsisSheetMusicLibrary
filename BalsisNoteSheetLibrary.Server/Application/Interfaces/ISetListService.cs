using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;

namespace BalsisNoteSheetLibrary.Server.Application.Interfaces;

public interface ISetListService
{
    Task<IEnumerable<SetListDto>> GetAllSetListsAsync(bool withNoteSheets = false);
    Task<IEnumerable<SetListDto>> GetAllArchivedSetListsAsync();
    Task<SetListDto?> GetSetListByIdAsync(uint id);
    Task<SetListDto> CreateSetListAsync(CreateSetListDto dto);
    Task<SetListDto> UpdateSetListAsync(UpdateSetListDto dto);
    Task DeleteSetListAsync(uint id);
    Task UpdateSetListOrderAsync(uint id, uint newOrder);
}