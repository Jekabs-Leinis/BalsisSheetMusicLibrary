namespace BalsisNoteSheetLibrary.Server.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName);
    bool FileExists(string fileName);
    string GetFilePath(string fileName);
    Task DeleteFile(string fileName);
    void RenameFile(string oldFileName, string newFileName);
}