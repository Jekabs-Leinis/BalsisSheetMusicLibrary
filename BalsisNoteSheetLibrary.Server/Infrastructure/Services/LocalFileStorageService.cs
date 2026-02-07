using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _basePath;

    public LocalFileStorageService(IHostEnvironment hostEnvironment, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        var sheetsFolderPath = Environment.GetEnvironmentVariable(EnvironmentVariables.SheetsFolderPath) ??
                               throw new InvalidOperationException(
                                   $"{EnvironmentVariables.SheetsFolderPath} environment variable must be set!");

        // First check if the path is absolute and exists, if so use it directly. Otherwise, treat it as relative to the content root.
        _basePath = Directory.Exists(sheetsFolderPath)
            ? sheetsFolderPath
            : Path.Combine(hostEnvironment.ContentRootPath, sheetsFolderPath);

        logger.LogInformation("Base path: {BasePath}", _basePath);

        try
        {
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create or access the sheets directory at path: {BasePath}", _basePath);

            throw;
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        try
        {
            _logger.LogDebug("Saving file: {FileName}", fileName);
            var filePath = Path.Combine(_basePath, fileName);
            await using var output = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(output);

            _logger.LogInformation("File saved successfully: {FileName}", fileName);
            return fileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file: {FileName}", fileName);
            throw;
        }
    }

    public bool FileExists(string fileName)
    {
        var filePath = Path.Combine(_basePath, fileName);
        var exists = File.Exists(filePath);
        _logger.LogDebug("File existence check for {FileName}: {Exists}", fileName, exists);

        return exists;
    }

    public string GetFilePath(string fileName)
    {
        return Path.Combine(_basePath, fileName);
    }

    public Task DeleteFile(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_basePath, fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogError("Attempted to delete non-existent file: {FileName}", fileName);

                throw new FileNotFoundException($"File not found: {fileName}");
            }

            File.Delete(filePath);
            _logger.LogInformation("File deleted successfully: {FileName}", fileName);

            return Task.CompletedTask;
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            _logger.LogError(ex, "Failed to delete file: {FileName}", fileName);
            throw;
        }
    }

    // There is no built-in method to rename a file async, but the synchronous method is fast enough for our use case.
    public void RenameFile(string oldFileName, string newFileName)
    {
        try
        {
            _logger.LogDebug("Renaming file from {OldFileName} to {NewFileName}", oldFileName, newFileName);
            var oldFilePath = Path.Combine(_basePath, oldFileName);

            if (!File.Exists(oldFilePath))
            {
                _logger.LogError("Cannot rename file: source file not found: {OldFileName}", oldFileName);

                throw new FileNotFoundException($"File not found: {oldFileName}");
            }

            var newFilePath = Path.Combine(_basePath, newFileName);

            File.Move(oldFilePath, newFilePath, true);
            _logger.LogInformation("File renamed successfully from {OldFileName} to {NewFileName}", oldFileName,
                newFileName);
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            _logger.LogError(ex, "Failed to rename file from {OldFileName} to {NewFileName}", oldFileName, newFileName);
            throw;
        }
    }
}