namespace BalsisNoteSheetLibrary.Server.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName);
    bool FileExists(string fileName);
    string GetFilePath(string fileName);
    Task DeleteFileAsync(string fileName);
    void MoveFile(string oldFileName, string newFileName);
}