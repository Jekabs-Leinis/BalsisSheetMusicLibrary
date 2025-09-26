using BalsisNoteSheetLibrary.Server.DTOs;
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

    public async Task<BaseResponseDto<IEnumerable<NoteSheetDto>>> GetAll()
    {
        var sheets = await context.NoteSheets
            .OrderBy(sheet => EF.Functions.Collate(sheet.Title, SqliteExtensions.InsensitiveCollation))
            .Select(sheet => NoteSheetDto.FromEntity(sheet))
            .ToListAsync();

        return new BaseResponseDto<IEnumerable<NoteSheetDto>>(sheets);
    }

    [HttpGet("{id:int}")]
    public async Task<BaseResponseDto<NoteSheetDto?>> Get(uint id)
    {
        var sheet = await context.NoteSheets.FindAsync(id);

        return new BaseResponseDto<NoteSheetDto?>(
            sheet is not null ? NoteSheetDto.FromEntity(sheet) : null,
            sheet is not null,
            sheet is null ? "Note sheet not found" : string.Empty
        );
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<BaseResponseDto<NoteSheetDto>> Add([FromForm] CreateNoteSheetDto createDto, IFormFile file)
    {
        if (file.Length == 0)
        {
            return new BaseResponseDto<NoteSheetDto>(null, false, "PDF file is required");
        }

        //TODO: check if this can be bypassed. We never want to serve a html file
        //Old version allowed images. Should we?
        if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            return new BaseResponseDto<NoteSheetDto>(null, false, "Only PDF files are allowed");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return new BaseResponseDto<NoteSheetDto>(null, false, string.Join(", ", errors));
        }

        var noteSheet = createDto.ToEntity();
        
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

        return new BaseResponseDto<NoteSheetDto>(NoteSheetDto.FromEntity(noteSheet));
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<BaseResponseDto<NoteSheetDto>> Update([FromForm] UpdateNoteSheetDto updateDto, IFormFile? file)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return new BaseResponseDto<NoteSheetDto>(null, false, string.Join(", ", errors));
        }

        var sheet = await context.NoteSheets.FindAsync(updateDto.Id);

        if (sheet is null)
        {
            return new BaseResponseDto<NoteSheetDto>(null, false, "Note sheet not found");
        }

        // Has to be done before file operations, to correctly generate the filename for the new file
        updateDto.UpdateEntity(sheet);
        
        var sheetsFolder = Path.Combine(env.ContentRootPath, "Static", "Sheets");
        var oldFilePath = Path.Combine(sheetsFolder, sheet.SystemFileName ?? "");

        if (file is { Length: > 0 })
        {
            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                return new BaseResponseDto<NoteSheetDto>(null, false, "Only PDF files are allowed");
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
                    //TODO: propper logging
                    //Old file missing? How?
                    Console.WriteLine($"Error deleting old file: {ex.Message}");
                }
            }
            
            var newFileName = GenerateFileName(sheet) + ".pdf";
            var newSystemFileName = $"{sheet.Id}_{newFileName}";
            var newFilePath = Path.Combine(sheetsFolder, newSystemFileName);

            await using (var fileStream = new FileStream(newFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            sheet.Filename = newFileName;
            sheet.SystemFileName = newSystemFileName;
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
                    var newFileName = GenerateFileName(sheet) + extension;
                    var newSystemFileName = $"{sheet.Id}_{newFileName}";
                    var newFilePath = Path.Combine(sheetsFolder, newSystemFileName);
                    System.IO.File.Move(oldFilePath, newFilePath, overwrite: true);
                    sheet.SystemFileName = newSystemFileName;
                    sheet.Filename = newFileName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error renaming file: {ex.Message}");
                    return new BaseResponseDto<NoteSheetDto>(null, false, $"Error renaming file: {ex.Message}");
                }
            }
            else
            {
                // TODO: no new file provided, but old file missing - what to do?
                // For now, just clear the filename fields as they are no longer valid
                // and to prevent user provided data from being saved
                sheet.SystemFileName = string.Empty;
                sheet.Filename = string.Empty;
            }
        }
        
        await context.SaveChangesAsync();

        return new BaseResponseDto<NoteSheetDto>(NoteSheetDto.FromEntity(sheet));
    }

    private static string GenerateFileName(NoteSheet sheet)
    {
        var nameParts = new List<string> { CleanFileName(sheet.Title ?? "MISSING TITLE") };

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

        return CleanFileName(fileName);
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
    public async Task<BaseResponseDto> Delete(uint id)
    {
        var sheet = await context.NoteSheets.FindAsync(id);

        if (sheet is null)
        {
            return new BaseResponseDto("Note sheet not found", false);
        }

        var sheetsFolder = Path.Combine(env.ContentRootPath, "Static", "Sheets");
        var filePath = Path.Combine(sheetsFolder, sheet.SystemFileName ?? "");

        if (System.IO.File.Exists(filePath))
        {
            try
            {
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
                //TODO: do not report db exceptions
                return new BaseResponseDto($"Error deleting file: {ex.Message}", false);
            }
        }

        context.NoteSheets.Remove(sheet);
        await context.SaveChangesAsync();

        return new BaseResponseDto("Note sheet deleted successfully");
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<IActionResult> RenameAllFilenames()
    {
        if (!await RenameLock.WaitAsync(0))
        {
            return Conflict(new BaseResponseDto("A rename process is already running.", false));
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
                        var isFileRenamed = false;
                        var fileToMove = System.IO.File.Exists(oldPath) ? oldPath :
                            System.IO.File.Exists(altPath) ? altPath : null;

                        if (fileToMove != null)
                        {
                            try
                            {
                                System.IO.File.Move(fileToMove, newPath, overwrite: true);
                                isFileRenamed = true;
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

                        if (isFileRenamed)
                        {
                            sheet.Filename = newFileName;
                            sheet.SystemFileName = newSystemFileName;
                            scopedContext.Update(sheet);
                            await scopedContext.SaveChangesAsync();
                        }

                        if ((current > 0 && current % 100 == 0) || current == total)
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
                //TODO: sometimes, service injection fails. Need to investigate
                Console.WriteLine($"Unexpected error in rename process: {ex.Message}");
            }
            finally
            {
                RenameLock.Release();
            }
        });

        return Ok(new BaseResponseDto("Rename process started in the background"));
    }
}