using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _basePath;
    private readonly string _trashPath;
    private readonly bool _softDeleteDisabled;

    public LocalFileStorageService(IHostEnvironment hostEnvironment, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        var sheetsFolderPath = Environment.GetEnvironmentVariable(EnvironmentVariables.SheetsFolderPath) ??
                               throw new InvalidOperationException(
                                   $"{EnvironmentVariables.SheetsFolderPath} environment variable must be set!");

        _basePath = Path.IsPathRooted(sheetsFolderPath)
            ? sheetsFolderPath
            : Path.Combine(hostEnvironment.ContentRootPath, sheetsFolderPath);

        logger.LogInformation("Base path: {BasePath}", _basePath);

        var softDeleteDisabledValue = Environment.GetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled);
        _softDeleteDisabled = softDeleteDisabledValue == "1";

        var trashFolderPath = Environment.GetEnvironmentVariable(EnvironmentVariables.TrashFolderPath);
        if (string.IsNullOrEmpty(trashFolderPath))
        {
            _trashPath = Path.Combine(_basePath, "trash");
        }
        else
        {
            _trashPath = Path.IsPathRooted(trashFolderPath)
                ? trashFolderPath
                : Path.Combine(hostEnvironment.ContentRootPath, trashFolderPath);
        }

        logger.LogInformation("Trash path: {TrashPath}, Soft delete disabled: {SoftDeleteDisabled}", _trashPath, _softDeleteDisabled);

        try
        {
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            if (!_softDeleteDisabled && !Directory.Exists(_trashPath))
            {
                Directory.CreateDirectory(_trashPath);
                logger.LogInformation("Created trash directory at: {TrashPath}", _trashPath);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create or access the sheets directory at path: {BasePath}", _basePath);

            throw;
        }
    }
    
    public string GetSafeFilePath(string fileName)
    {
        // Validate filename
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Filename cannot be empty", nameof(fileName));
    
        // Remove any path components
        var safeFileName = Path.GetFileName(fileName);
    
        var filePath = Path.Combine(_basePath, safeFileName);
    
        // Ensure the resolved path is still within base path
        var fullPath = Path.GetFullPath(filePath);
        if (!fullPath.StartsWith(Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid file path", nameof(fileName));
    
        return fullPath;
    }
    
    public string GetBasePath()
    {
        return _basePath;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (!fileStream.CanRead)
            throw new ArgumentException("Stream must be readable", nameof(fileStream));

        try
        {
            _logger.LogDebug("Saving file: {FileName}", fileName);
            var filePath = GetSafeFilePath(fileName);

            await using var output = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
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
        var filePath = GetSafeFilePath(fileName);
        var exists = File.Exists(filePath);
        _logger.LogDebug("File existence check for {FileName}: {Exists}", fileName, exists);

        return exists;
    }

    public Task DeleteFile(string fileName, string reason = "manual", bool forcePermanent = false)
    {
        try
        {
            var filePath = GetSafeFilePath(fileName);

            if (!File.Exists(filePath))
            {
                _logger.LogError("Attempted to delete non-existent file: {FileName}", fileName);

                throw new FileNotFoundException($"File not found: {fileName}");
            }

            if (_softDeleteDisabled || forcePermanent)
            {
                File.Delete(filePath);
                _logger.LogInformation("File permanently deleted: {FileName}", fileName);
            }
            else
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                var extension = Path.GetExtension(fileName);
                var trashFileName = $"{fileNameWithoutExtension}_{timestamp}_{reason}{extension}";
                var trashFilePath = Path.Combine(_trashPath, trashFileName);

                File.Move(filePath, trashFilePath);
                _logger.LogInformation("File soft-deleted: {FileName} -> {TrashFileName}", fileName, trashFileName);
            }

            return Task.CompletedTask;
        }
        catch (Exception ex) when (ex is not FileNotFoundException)
        {
            _logger.LogError(ex, "Failed to delete file: {FileName}", fileName);
            throw;
        }
    }

    // There is no built-in method to rename a file async, but the synchronous method is fast enough for our use case as it's just a metadata change on the filesystem table.
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

            var newFilePath = GetSafeFilePath(newFileName);

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