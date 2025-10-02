using BalsisNoteSheetLibrary.Server.Api.Controllers;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BalsisNoteSheetLibrary.Server.Application.Services;

public class NoteSheetRenameService(IServiceProvider serviceProvider) : INoteSheetRenameService
{
    private static readonly SemaphoreSlim RenameLock = new(1, 1);

    public async Task RenameAllFilenamesAsync()
    {
        if (!await RenameLock.WaitAsync(0))
        {
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
            }
        });
    }

    private async Task RunRenameAllFilenamesInScope(IServiceProvider scopeProvider)
    {
        try
        {
            await using var scope = scopeProvider.CreateAsyncScope();
            var scopedContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var scopedRenameHub = scope.ServiceProvider.GetRequiredService<IHubContext<StatusHub>>();
            var scopedEnv = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var scopedFileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

            SendStatus(scopedRenameHub, "start", "Renaming started.");
            var sheets = scopedContext.NoteSheets.ToList();
            var sheetsFolder = Path.Combine(scopedEnv.ContentRootPath, "Static", "Sheets");
            await RenameAllSheets(sheets, sheetsFolder, scopedContext, scopedFileStorageService, scopedRenameHub);
            SendStatus(scopedRenameHub, "complete", "Renaming complete.");
        }
        catch (Exception ex)
        {
            // TODO: Log the exception
        }
    }

    private async Task RenameAllSheets(List<NoteSheet> sheets, string sheetsFolder, AppDbContext scopedContext,
        IFileStorageService fileStorage, IHubContext<StatusHub> hub)
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
                    SendStatus(hub, "error", $"File not found for sheet {sheet.Id}");
                }
            }
            catch (Exception ex)
            {
                SendStatus(hub, "error", $"Error renaming file for sheet {sheet.Id}: {ex.Message}");
            }

            if ((current > 0 && current % 100 == 0) || current == total)
            {
                SendStatus(hub, "progress", $"Renamed {current}/{total}", current, total);
            }
        }
    }

    private async Task<bool> RenameSingleSheet(NoteSheet sheet, string sheetsFolder, AppDbContext scopedContext,
        IFileStorageService fileStorage)
    {
        var newFileName = fileStorage.GetFileName(sheet);
        var newSystemFileName = fileStorage.GetSystemFileName(sheet);
        var oldPath = Path.Combine(sheetsFolder, sheet.SystemFileName ?? "");
        var newPath = Path.Combine(sheetsFolder, newSystemFileName);

        try
        {
            fileStorage.MoveFile(oldPath, newPath);
        }
        catch (FileNotFoundException)
        {
            return false;
        }

        sheet.FileName = newFileName;
        sheet.SystemFileName = newSystemFileName;
        scopedContext.Update(sheet);
        await scopedContext.SaveChangesAsync();

        return true;
    }

    private void SendStatus(IHubContext<StatusHub> hub, string status, string message, int? current = null,
        int? total = null)
    {
        if (current.HasValue && total.HasValue)
        {
            // While this is not awaited, it's acceptable in this context as we don't need or want to block on it.
            hub.Clients.All.SendAsync("status", new { status, current, total, message });
        }
        else
        {
            hub.Clients.All.SendAsync("status", new { status, message });
        }
    }
}