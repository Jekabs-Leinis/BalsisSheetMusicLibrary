using BalsisNoteSheetLibrary.Server.Application.DTOs;
using System.Threading;

namespace BalsisNoteSheetLibrary.Server.Application.Interfaces;

public interface INoteSheetService
{
    Task<NoteSheetDto?> GetNoteSheetAsync(uint id);
    Task<IEnumerable<NoteSheetDto>> GetAllNoteSheetsAsync();
    Task<NoteSheetDto> CreateNoteSheetAsync(CreateNoteSheetDto dto, Stream fileStream);
    Task<NoteSheetDto> UpdateNoteSheetAsync(UpdateNoteSheetDto dto, Stream? fileStream);
    Task DeleteNoteSheetAsync(uint id);
}