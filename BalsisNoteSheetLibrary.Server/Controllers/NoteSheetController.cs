using BalsisNoteSheetLibrary.Server.Helpers;
using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class NoteSheetController(AppDbContext context, IWebHostEnvironment env) : ControllerBase
{
    public async Task<AppResponse<IEnumerable<NoteSheet>>> GetAll()
    {
        var sheets = await context.NoteSheets.OrderBy(sheet =>
            EF.Functions.Collate(sheet.Title, SqliteExtensions.InsensitiveCollation))
            .ToArrayAsync();

        return new AppResponse<IEnumerable<NoteSheet>>(sheets, true);
    }

    [HttpGet("{id:int}")]
    public async Task<AppResponse<NoteSheet?>> Get(uint id)
    {
        var sheet = await context.NoteSheets.FindAsync(id);

        return new AppResponse<NoteSheet?>(
            sheet,
            sheet is not null,
            sheet is null ? "Note sheet not found" : string.Empty
        );
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<AppResponse<string>> Add(NoteSheet noteSheet)
    {
        context.NoteSheets.Add(noteSheet);
        await context.SaveChangesAsync();

        return new AppResponse<string>("Note sheet added", true);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<AppResponse<NoteSheet>> Update([FromForm] NoteSheet noteSheet, IFormFile? file)
    {
        var sheet = await context.NoteSheets.FindAsync(noteSheet.Id);

        if (sheet is null)
        {
            return new AppResponse<NoteSheet>(null, false, "Note sheet not found");
        }
        
        if (file is { Length: > 0 })
        {
            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return new AppResponse<NoteSheet>(null, false, "Only PDF files are allowed");
            }
            
            var sheetsFolder = Path.Combine(env.ContentRootPath, "Static", "Sheets");

            // Generate a filename based on sheet metadata
            var fileName = $"{GenerateFileName(noteSheet)}.pdf";
            var systemFileName = $"{noteSheet.Id}_{fileName}";
            
            var filePath = Path.Combine(sheetsFolder, systemFileName);
            
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            
            if (!string.IsNullOrEmpty(sheet.Filename))
            {
                var oldFilePath = Path.Combine(sheetsFolder, sheet.GetSystemFileName());
                if (System.IO.File.Exists(oldFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    catch (Exception ex)
                    {
                        // New file was saved, but old file could not be deleted
                        // This is kind of okay, but we should log the error
                        Console.WriteLine($"Error deleting old file: {ex.Message}");
                    }
                }
            }
            
            noteSheet.Filename = fileName;
        }

        context.Entry(sheet).CurrentValues.SetValues(noteSheet);
        await context.SaveChangesAsync();

        return new AppResponse<NoteSheet>(sheet, true);
    }

    // Helper method to generate a clean filename based on sheet metadata
    private static string GenerateFileName(NoteSheet sheet)
    {
        // Title is mandatory
        var nameParts = new List<string> { CleanFileName(sheet.Title ?? "MISSING_TITLE") };
        
        if (!string.IsNullOrWhiteSpace(sheet.Author))
        {
            nameParts.Add(CleanFileName(sheet.Author));
        }
        
        if (!string.IsNullOrWhiteSpace(sheet.Lyricist))
        {
            nameParts.Add(CleanFileName(sheet.Lyricist));
        }
        
        if (sheet.Year is not null)
        {
            nameParts.Add(sheet.Year.ToString());
        }
        
        var fileName = string.Join(", ", nameParts);
        
        // Ensure the filename isn't too long (Windows max path is 260, but we'll limit filename to 200)
        if (fileName.Length > 200)
        {
            fileName = fileName[..200];
        }
        
        return fileName;
    }
    
    private static string CleanFileName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
        
        var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        invalidChars += "#"; // Block # as it is used in browser URLs as anchor and is not sent to server
        var invalidRegex = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
        
        return Regex.Replace(input, invalidRegex, "").Trim();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<AppResponse<string>> Delete(uint id)
    {
        var sheet = await context.NoteSheets.FindAsync(id);

        if (sheet is null)
        {
            return new AppResponse<string>(null, false, "Note sheet not found");
        }

        context.NoteSheets.Remove(sheet);
        await context.SaveChangesAsync();

        return new AppResponse<string>("Note sheet deleted", true);
    }
}