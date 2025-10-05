using BalsisNoteSheetLibrary.Server.Infrastructure.Services.Interfaces;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IHostEnvironment hostEnvironment)
    {
        _basePath = Path.Combine(hostEnvironment.ContentRootPath, "Static", "Sheets");
        ;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        var filePath = Path.Combine(_basePath, fileName);
        await using var output = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(output);

        return fileName;
    }

    public bool FileExists(string fileName)
    {
        var filePath = Path.Combine(_basePath, fileName);

        return File.Exists(filePath);
    }

    public Task DeleteFileAsync(string fileName)
    {
        var filePath = Path.Combine(_basePath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public void MoveFile(string oldFileName, string newFileName)
    {
        var oldFilePath = Path.Combine(_basePath, oldFileName);

        if (!File.Exists(oldFilePath))
        {
            throw new FileNotFoundException();
        }

        var newFilePath = Path.Combine(_basePath, newFileName);

        File.Move(oldFilePath, newFilePath, true);
    }
}