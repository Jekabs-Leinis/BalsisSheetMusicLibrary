namespace BalsisSheetMusicLibrary.Server.Application.Interfaces;

public interface INoteSheetRenameService
{
    Task RenameAllFilenamesAsync();
}