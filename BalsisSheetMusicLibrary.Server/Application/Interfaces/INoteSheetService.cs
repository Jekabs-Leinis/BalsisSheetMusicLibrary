using BalsisSheetMusicLibrary.Server.Application.DTOs.NoteSheet;

namespace BalsisSheetMusicLibrary.Server.Application.Interfaces;

public interface INoteSheetService
{
    Task<NoteSheetDto?> GetNoteSheetAsync(uint id);
    bool HasValidFile(NoteSheetDto dto);
    Task<List<NoteSheetDto>> GetAllNoteSheetsAsync();
    Task<NoteSheetDto> CreateNoteSheetAsync(CreateNoteSheetDto dto, Stream fileStream);
    Task<NoteSheetDto> UpdateNoteSheetAsync(UpdateNoteSheetDto dto, Stream? fileStream);
    Task DeleteNoteSheetAsync(uint id);
}