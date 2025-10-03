using System.Text.RegularExpressions;
using BalsisNoteSheetLibrary.Server.Domain.Entities;
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

    public Stream GetFile(string fileName)
    {
        var filePath = Path.Combine(_basePath, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException();
        }

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
            true);
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

    public string GetFileName(NoteSheet sheet)
    {
        var nameParts = new List<string> { CleanFileName(sheet.Title ?? "MISSING TITLE") };

        if (!string.IsNullOrWhiteSpace(sheet.Author))
        {
            nameParts.Add(sheet.Author);
        }

        if (!string.IsNullOrWhiteSpace(sheet.Lyricist))
        {
            nameParts.Add(sheet.Lyricist);
        }

        if (sheet.Year is not null)
        {
            nameParts.Add(sheet.Year.ToString() ?? string.Empty);
        }

        var fileName = string.Join(", ", nameParts);

        fileName = CleanFileName(fileName);

        // Windows paths have a maximum length of 260 characters,
        // but filenames should be shorter to account for folder paths
        if (fileName.Length > 200)
        {
            fileName = fileName[..200];
        }

        if (fileName.Length == 0)
        {
            throw new InvalidOperationException("File name cannot be empty.");
        }

        return fileName + ".pdf";
    }

    public string GetSystemFileName(NoteSheet sheet)
    {
        if (sheet.Id is null)
        {
            throw new InvalidOperationException("Sheet id cannot be null.");
        }

        // If filename has already been set, trust it
        return string.IsNullOrEmpty(sheet.FileName)
            ? $"{sheet.Id}_{GetFileName(sheet)}"
            : $"{sheet.Id}_{sheet.FileName}";
    }

    private static string CleanFileName(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }
        
        // Remove invalid filename characters
        // Also remove trailing dots and spaces to avoid issues on Windows
        // Path.GetInvalidFileNameChars() is not usable here as it is file system dependent,
        // but the files we want to serve should be usable on any system
        var invalidCharsArray = new[]
        {
            '\"', '<', '>', '|', '\0',
            (char)1, (char)2, (char)3, (char)4, (char)5, (char)6, (char)7, (char)8, (char)9, (char)10,
            (char)11, (char)12, (char)13, (char)14, (char)15, (char)16, (char)17, (char)18, (char)19, (char)20,
            (char)21, (char)22, (char)23, (char)24, (char)25, (char)26, (char)27, (char)28, (char)29, (char)30,
            (char)31, ':', '*', '?', '\\', '/', '#'
        };
        var invalidChars = Regex.Escape(new string(invalidCharsArray));
        var invalidRegex = $@"([{invalidChars}]+)|([\. ]+$)";
        
        // Prevent directory traversal by stripping path components
        input = Path.GetFileName(input);
        
        return Regex.Replace(input, invalidRegex, "").Trim();
    }
}