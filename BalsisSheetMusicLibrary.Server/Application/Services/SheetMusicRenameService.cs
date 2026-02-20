using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.Entities;
using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using BalsisSheetMusicLibrary.Server.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BalsisSheetMusicLibrary.Server.Application.Services;

public class SheetMusicMusicRenameService(IServiceProvider serviceProvider, ILogger<SheetMusicMusicRenameService> logger)
    : ISheetMusicRenameService
{
    private static readonly SemaphoreSlim RenameLock = new(1, 1);

    public async Task RenameAllFilenamesAsync()
    {
        logger.LogInformation("Starting rename operation for all sheet music filenames.");

        if (!await RenameLock.WaitAsync(0))
        {
            logger.LogInformation("Rename operation already in progress. Exiting.");

            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await RenameAllFilenamesTask(serviceProvider);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception in rename operation");
            }
            finally
            {
                RenameLock.Release();
                logger.LogInformation("Rename operation completed.");
            }
        }).ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                logger.LogError(t.Exception, "Task faulted in rename operation");
            }
        }, TaskScheduler.Default);
    }

    private async Task RenameAllFilenamesTask(IServiceProvider scopeProvider)
    {
        try
        {
            await using var scope = scopeProvider.CreateAsyncScope();
            var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var scopedRenameHub = scope.ServiceProvider.GetRequiredService<IHubContext<StatusHub>>();
            var scopedFileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
            // Intentionally not awaited, as we don't want to block on this.
            _ = SendStatus(scopedRenameHub, "start", "Pārsaukšana uzsākta.");
            var sheets = await scopedUnitOfWork.SheetMusic.GetAllAsync(); ;
            await RenameAllSheets(sheets, scopedUnitOfWork, scopedFileStorageService, scopedRenameHub);
            _ = SendStatus(scopedRenameHub, "complete", "Pāršaukšana pabeigta.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error renaming sheet music");
        }
    }

    private async Task RenameAllSheets(List<SheetMusic> sheets, IUnitOfWork scopedUnitOfWork,
        IFileStorageService fileStorage, IHubContext<StatusHub> scopedHub)
    {
        var total = sheets.Count;
        var current = 0;

        foreach (var sheet in sheets)
        {
            try
            {
                var renamed = await RenameSingleSheet(sheet, scopedUnitOfWork, fileStorage);

                if (!renamed)
                {
                    _ = SendStatus(scopedHub, "error", $"Notīm \"{sheet.Title}\" neizdvās atrast failu.");
                }
                else
                {
                    current++;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error renaming sheet music");

                _ = SendStatus(scopedHub, "error", $"Radās kļūda pārsaucot notis {sheet.Title}: {ex.Message}");
            }

            if ((current > 0 && current % 100 == 0) || current == total)
            {
                _ = SendStatus(scopedHub, "progress", $"Pārsauktas {current}/{total}");
            }
        }
    }

    private async Task<bool> RenameSingleSheet(SheetMusic sheetMusic, IUnitOfWork scopedUnitOfWork,
        IFileStorageService fileStorage)
    {
        var oldExtension = Path.GetExtension(sheetMusic.SystemFileName);
        var newFileName = sheetMusic.GetFileName(oldExtension);
        var newSystemFileName = sheetMusic.GetSystemFileName(oldExtension);
        var sheetsFolder = fileStorage.GetBasePath();
        var oldPath = Path.Combine(sheetsFolder, sheetMusic.SystemFileName ?? "");
        var newPath = Path.Combine(sheetsFolder, newSystemFileName);

        try
        {
            fileStorage.RenameFile(oldPath, newPath);
        }
        catch (FileNotFoundException ex)
        {
            logger.LogError(ex, "File not found for SheetMusic ID {Id} at path {Path}", sheetMusic.Id, oldPath);

            return false;
        }

        sheetMusic.FileName = newFileName;
        sheetMusic.SystemFileName = newSystemFileName;
        scopedUnitOfWork.SheetMusic.Update(sheetMusic);
        await scopedUnitOfWork.SaveChangesAsync();

        return true;
    }

    private static async Task SendStatus(IHubContext<StatusHub> hub, string status, string message)
    {
        // Should only produce notification for clients that are on the rename page, so sending to all is ok.
        await hub.Clients.All.SendAsync("status", new { status, message });
    }
}