using BalsisNoteSheetLibrary.Server.Helpers;
using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class NoteSheetController(AppDbContext context, IWebHostEnvironment env, IServiceProvider sp)
    : ControllerBase
{
    private static readonly SemaphoreSlim RenameLock = new(1, 1);

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
    public async Task<AppResponse<NoteSheet>> Add([FromForm] NoteSheet noteSheet, IFormFile file)
    {
        if (file.Length == 0)
        {
            return new AppResponse<NoteSheet>(null, false, "PDF file is required");
        }

        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return new AppResponse<NoteSheet>(null, false, "Only PDF files are allowed");
        }

        // Add the sheet here to get an ID assigned, for filename
        context.NoteSheets.Add(noteSheet);
        await context.SaveChangesAsync();

        var sheetsFolder = Path.Combine(env.ContentRootPath, "Static", "Sheets");
        Directory.CreateDirectory(sheetsFolder);

        var fileName = GenerateFileName(noteSheet) + ".pdf";
        var systemFileName = $"{noteSheet.Id}_{fileName}";
        var filePath = Path.Combine(sheetsFolder, systemFileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        noteSheet.Filename = fileName;
        noteSheet.SystemFileName = systemFileName;
        await context.SaveChangesAsync();

        return new AppResponse<NoteSheet>(noteSheet, true);
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

        var sheetsFolder = Path.Combine(env.ContentRootPath, "Static", "Sheets");
        var oldFilePath = Path.Combine(sheetsFolder, sheet.SystemFileName ?? "");

        if (file is { Length: > 0 })
        {
            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return new AppResponse<NoteSheet>(null, false, "Only PDF files are allowed");
            }


            // Delete the old file if it exists
            if (System.IO.File.Exists(oldFilePath))
            {
                try
                {
                    System.IO.File.Delete(oldFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting old file: {ex.Message}");
                }
            }
            
            var newFileName = GenerateFileName(noteSheet) + ".pdf";
            var newSystemFileName = $"{sheet.Id}_{newFileName}";
            var newFilePath = Path.Combine(sheetsFolder, newSystemFileName);

            await using (var fileStream = new FileStream(newFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            noteSheet.Filename = newFileName;
            noteSheet.SystemFileName = newSystemFileName;
        }
        else
        {
            // Rename the existing file, with the expectation that data (from which filename is generated) has changed
            if (System.IO.File.Exists(oldFilePath))
            {
                try
                {
                    // The previous version allowed any file extension, so we need to preserve it
                    // A future update could standardize all files to .pdf
                    var extension = Path.GetExtension(oldFilePath);
                    var newFileName = GenerateFileName(noteSheet) + extension;
                    var newSystemFileName = $"{sheet.Id}_{newFileName}";
                    var newFilePath = Path.Combine(sheetsFolder, newSystemFileName);
                    System.IO.File.Move(oldFilePath, newFilePath, overwrite: true);
                    noteSheet.SystemFileName = newSystemFileName;
                    noteSheet.Filename = newFileName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error renaming file: {ex.Message}");
                    return new AppResponse<NoteSheet>(null, false, $"Error renaming file: {ex.Message}");
                }
            }
            else
            {
                // TODO: file missing - what to do?
                // For now, just clear the filename fields as they are no longer valid
                // and to prevent user provided data from being saved
                noteSheet.SystemFileName = string.Empty;
                noteSheet.Filename = string.Empty;
            }
        }

        context.Entry(sheet).CurrentValues.SetValues(noteSheet);
        await context.SaveChangesAsync();

        return new AppResponse<NoteSheet>(sheet, true);
    }

    private static string GenerateFileName(NoteSheet sheet)
    {
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
            nameParts.Add(sheet.Year.ToString() ?? string.Empty);
        }

        var fileName = string.Join(", ", nameParts);

        // Windows paths have a maximum length of 260 characters,
        // but filenames should be shorter to account for folder paths
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
        invalidChars += "#"; // Also remove '#' to avoid URL encoding issues
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

        var sheetsFolder = Path.Combine(env.ContentRootPath, "Static", "Sheets");
        var filePath = Path.Combine(sheetsFolder, sheet.SystemFileName ?? "");

        if (System.IO.File.Exists(filePath))
        {
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch
            {
                // TODO: figure out what to do if file deletion fails
            }
        }

        context.NoteSheets.Remove(sheet);
        await context.SaveChangesAsync();

        return new AppResponse<string>("Note sheet deleted", true);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> RenameAllFilenames()
    {
        if (!await RenameLock.WaitAsync(0))
        {
            return Conflict(new AppResponse<string>(null, false, "A rename process is already running."));
        }

        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = sp.CreateScope();
                var scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var scopedRenameHub = scope.ServiceProvider.GetRequiredService<IHubContext<StatusHub>>();
                var scopedEnv = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                try
                {
                    await scopedRenameHub.Clients.All.SendAsync("status",
                        new { status = "start", message = "Renaming started." });
                    var sheets = scopedContext.NoteSheets.ToList();
                    var sheetsFolder = Path.Combine(scopedEnv.ContentRootPath, "Static", "Sheets");
                    var total = sheets.Count;
                    var current = 0;

                    foreach (var sheet in sheets)
                    {
                        current++;
                        // The previous version allowed any file extension, so we need to preserve it
                        // A future update could standardize all files to .pdf
                        var extension = Path.GetExtension(sheet.SystemFileName) ?? ".pdf";
                        var newFileName = GenerateFileName(sheet) + extension;
                        var newSystemFileName = $"{sheet.Id}_{newFileName}";
                        var oldPath = Path.Combine(sheetsFolder, sheet.SystemFileName ?? "");
                        // Path fallback for files that were imported from the old system without the ID prefix
                        // A future update could remove this fallback
                        var altPath = Path.Combine(sheetsFolder, sheet.Filename ?? string.Empty);
                        var newPath = Path.Combine(sheetsFolder, newSystemFileName);
                        bool renamed = false;
                        var fileToMove = System.IO.File.Exists(oldPath) ? oldPath :
                            System.IO.File.Exists(altPath) ? altPath : null;

                        if (fileToMove != null)
                        {
                            try
                            {
                                System.IO.File.Move(fileToMove, newPath, overwrite: true);
                                renamed = true;
                            }
                            catch (Exception ex)
                            {
                                await scopedRenameHub.Clients.All.SendAsync("status",
                                    new
                                    {
                                        status = "error",
                                        message = $"Error renaming file for sheet {sheet.Id}: {ex.Message}"
                                    });
                            }
                        }

                        if (renamed)
                        {
                            sheet.Filename = newFileName;
                            sheet.SystemFileName = newSystemFileName;
                            scopedContext.Update(sheet);
                            await scopedContext.SaveChangesAsync();
                        }

                        if (current > 0 && current % 100 == 0)
                        {
                            await scopedRenameHub.Clients.All.SendAsync("status",
                                new { status = "progress", current, total, message = $"Renamed {current}/{total}" });
                        }
                    }

                    await scopedRenameHub.Clients.All.SendAsync("status",
                        new { status = "complete", message = "Renaming complete." });
                }
                catch (Exception ex)
                {
                    await scopedRenameHub.Clients.All.SendAsync("status",
                        new { status = "error", message = $"Rename process failed: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in rename process: {ex.Message}");
            }
            finally
            {
                RenameLock.Release();
                Console.WriteLine("Rename process finished.");
            }
        });

        return Ok(new AppResponse<string>(null, true));
    }
}