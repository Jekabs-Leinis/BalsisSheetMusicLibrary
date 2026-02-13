namespace BalsisSheetMusicLibrary.Server.Application.Interfaces;

public interface ISheetMusicRenameService
{
    Task RenameAllFilenamesAsync();
}