namespace BalsisSheetMusicLibrary.Server.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName);
    bool FileExists(string fileName);
    string GetSafeFilePath(string fileName);
    string GetBasePath();
    Task DeleteFile(string fileName, string reason = "manual", bool forcePermanent = false);
    void RenameFile(string oldFileName, string newFileName);
}