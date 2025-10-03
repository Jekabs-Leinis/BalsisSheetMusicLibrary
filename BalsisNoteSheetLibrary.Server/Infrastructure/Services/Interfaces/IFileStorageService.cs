namespace BalsisNoteSheetLibrary.Server.Infrastructure.Services.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName);
    Stream GetFile(string fileName);
    Task DeleteFileAsync(string fileName);
    void MoveFile(string oldFileName, string newFileName);
    
    string GetFileName(Domain.Entities.NoteSheet sheet);
    
    string GetSystemFileName(Domain.Entities.NoteSheet sheet);
}
