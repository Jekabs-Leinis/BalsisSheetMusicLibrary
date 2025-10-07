using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using BalsisNoteSheetLibrary.Server.Infrastructure.Hubs;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services.Interfaces;

namespace BalsisNoteSheetLibrary.Server.Application.Services;

public class NoteSheetRenameService(IServiceProvider serviceProvider, ILogger<NoteSheetRenameService> logger) : INoteSheetRenameService
{
    private static readonly SemaphoreSlim RenameLock = new(1, 1);

    public async Task RenameAllFilenamesAsync()
    {
        logger.LogInformation("Starting rename operation for all note sheet filenames.");
        
        if (!await RenameLock.WaitAsync(0))
        {
            logger.LogInformation("Rename operation already in progress. Exiting.");
            
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await RunRenameAllFilenamesInScope(serviceProvider);
            }
            finally
            {
                RenameLock.Release();
                
                logger.LogInformation("Rename operation completed.");
            }
        });
    }

    private async Task RunRenameAllFilenamesInScope(IServiceProvider scopeProvider)
    {
        try
        {
            await using var scope = scopeProvider.CreateAsyncScope();
            var scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var scopedRenameHub = scope.ServiceProvider.GetRequiredService<StatusHub>();
            var scopedEnv = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var scopedFileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
            // Intentionally not awaited, as we don't want to block on this.
            _ = scopedRenameHub.SendStatus("start", "Renaming started.");
            var sheets = scopedContext.NoteSheets.ToList();
            var sheetsFolder = Path.Combine(scopedEnv.ContentRootPath, "Static", "Sheets");
            await RenameAllSheets(sheets, sheetsFolder, scopedContext, scopedFileStorageService, scopedRenameHub);
            _ = scopedRenameHub.SendStatus("complete", "Renaming complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error renaming note sheets");
        }
    }

    private async Task RenameAllSheets(List<NoteSheet> sheets, string sheetsFolder, AppDbContext scopedContext,
        IFileStorageService fileStorage, StatusHub hub)
    {
        var total = sheets.Count;
        var current = 0;

        foreach (var sheet in sheets)
        {
            current++;

            try
            {
                var renamed = await RenameSingleSheet(sheet, sheetsFolder, scopedContext, fileStorage);

                if (!renamed)
                {
                    _ = hub.SendStatus("error", $"File not found for sheet {sheet.Id}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error renaming note sheets");
                
                _ = hub.SendStatus("error", $"Error renaming file for sheet {sheet.Id}: {ex.Message}");
            }

            if ((current > 0 && current % 100 == 0) || current == total)
            {
                _ = hub.SendStatus("progress", $"Renamed {current}/{total}", current, total);
            }
        }
    }

    private async Task<bool> RenameSingleSheet(NoteSheet sheet, string sheetsFolder, AppDbContext scopedContext,
        IFileStorageService fileStorage)
    {
        var newFileName = sheet.GetFileName();
        var newSystemFileName = sheet.GetSystemFileName();
        var oldPath = Path.Combine(sheetsFolder, sheet.SystemFileName ?? "");
        var newPath = Path.Combine(sheetsFolder, newSystemFileName);

        try
        {
            fileStorage.MoveFile(oldPath, newPath);
        }
        catch (FileNotFoundException ex)
        {
            logger.LogError(ex, "File not found for NoteSheet ID {Id} at path {Path}", sheet.Id, oldPath);
            
            return false;
        }

        sheet.FileName = newFileName;
        sheet.SystemFileName = newSystemFileName;
        scopedContext.Update(sheet);
        await scopedContext.SaveChangesAsync();

        return true;
    }
}