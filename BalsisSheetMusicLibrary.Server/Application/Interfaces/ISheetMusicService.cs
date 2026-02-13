using BalsisSheetMusicLibrary.Server.Application.DTOs.SheetMusic;

namespace BalsisSheetMusicLibrary.Server.Application.Interfaces;

public interface ISheetMusicService
{
    Task<SheetMusicDto?> GetSheetMusicAsync(uint id);
    bool HasValidFile(SheetMusicDto sheetDto);
    Task<List<SheetMusicDto>> GetAllSheetMusicAsync();
    Task<SheetMusicDto> CreateSheetMusicAsync(CreateSheetMusicDto sheetDto, Stream fileStream);
    Task<SheetMusicDto> UpdateSheetMusicAsync(UpdateSheetMusicDto sheetDto, Stream? fileStream);
    Task DeleteSheetMusicAsync(uint id);
}