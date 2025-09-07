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

        // Add the sheet to get an ID assigned
        context.NoteSheets.Add(noteSheet);
        await context.SaveChangesAsync();

        var sheetsFolder = Path.Combine(env.ContentRootPath, "Static", "Sheets");
        Directory.CreateDirectory(sheetsFolder);

        var fileName = SaveSheetFile(noteSheet, file, sheetsFolder);
        noteSheet.Filename = fileName;

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
        var oldFilePath = Path.Combine(sheetsFolder, sheet.GetSystemFileName());

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
            else
            {
                // TODO: Log file somehow missing
            }

            noteSheet.Filename = SaveSheetFile(noteSheet, file, sheetsFolder);
        }
        else
        {
            // Rename the existing file, if data has changed
            if (System.IO.File.Exists(oldFilePath))
            {
                try
                {
                    var newFilePath = Path.Combine(sheetsFolder, $"{sheet.Id}_{GenerateFileName(noteSheet)}");
                    System.IO.File.Move(oldFilePath, newFilePath, overwrite: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error renaming file: {ex.Message}");
                }
            }

            noteSheet.Filename = GenerateFileName(noteSheet);
        }

        context.Entry(sheet).CurrentValues.SetValues(noteSheet);
        await context.SaveChangesAsync();

        return new AppResponse<NoteSheet>(sheet, true);
    }

    private string SaveSheetFile(NoteSheet noteSheet, IFormFile file, string targetFolder)
    {
        var fileName = GenerateFileName(noteSheet);
        var systemFileName = $"{noteSheet.Id}_{fileName}";
        var filePath = Path.Combine(targetFolder, systemFileName);

        using var fileStream = new FileStream(filePath, FileMode.Create);

        file.CopyTo(fileStream);

        return fileName;
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

        return fileName + ".pdf";
    }

    private static string CleanFileName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
        invalidChars += "#";
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
                        var newFileName = GenerateFileName(sheet);
                        var newSystemFileName = $"{sheet.Id}_{newFileName}";
                        var oldSystemFileName = sheet.GetSystemFileName();
                        var oldPath = Path.Combine(sheetsFolder, oldSystemFileName);
                        // Old path fallback for files that were imported from the old system without the ID prefix
                        var altPath = Path.Combine(sheetsFolder, sheet.Filename ?? string.Empty);
                        var newPath = Path.Combine(sheetsFolder, newSystemFileName);
                        bool renamed = false;

                        var fileToMove = System.IO.File.Exists(oldPath) ? oldPath
                            : System.IO.File.Exists(altPath) ? altPath
                            : null;

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