using BalsisSheetMusicLibrary.Server.Application.DTOs.SheetMusic;
using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BalsisSheetMusicLibrary.Server.Application.Services;

public class SheetMusicMusicService(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService)
    : ISheetMusicService
{
    public async Task<SheetMusicDto?> GetSheetMusicAsync(uint id)
    {
        var sheetMusic = await unitOfWork.SheetMusic.GetByIdAsync(id);

        return sheetMusic != null ? SheetMusicDto.FromEntity(sheetMusic) : null;
    }

    public async Task<List<SheetMusicDto>> GetAllSheetMusicAsync()
    {
        var sheetMusic = await unitOfWork.SheetMusic.GetAsync(
            orderBy: sheet => EF.Functions.Collate(sheet.Title, SqliteExtensions.InsensitiveCollation)!
        );

        return sheetMusic.Select(SheetMusicDto.FromEntity).ToList();
    }

    public async Task<SheetMusicDto> CreateSheetMusicAsync(CreateSheetMusicDto sheetDto, Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        
        var sheetMusic = sheetDto.ToEntity();
        unitOfWork.SheetMusic.Add(sheetMusic);
        // Save initially to generate ID for filename
        await unitOfWork.SaveChangesAsync();

        sheetMusic.FileName = sheetMusic.GetFileName();
        sheetMusic.SystemFileName = sheetMusic.GetSystemFileName();
        
        try
        {
            await fileStorageService.SaveFileAsync(fileStream, sheetMusic.SystemFileName);
            await unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // Rollback: permanently delete the file if it was created (not soft-delete)
            if (!string.IsNullOrEmpty(sheetMusic.SystemFileName))
            {
                await fileStorageService.DeleteFile(sheetMusic.SystemFileName, forcePermanent: true);
            }
            // Remove the entity from tracking
            unitOfWork.SheetMusic.Remove(sheetMusic);
            await unitOfWork.SaveChangesAsync();
            
            throw;
        }

        return SheetMusicDto.FromEntity(sheetMusic);
    }

    public async Task<SheetMusicDto> UpdateSheetMusicAsync(UpdateSheetMusicDto sheetDto, Stream? fileStream)
    {
        var sheetMusic = await unitOfWork.SheetMusic.GetByIdAsync(sheetDto.Id);

        if (sheetMusic == null)
        {
            throw new InvalidOperationException("SheetMusic not found");
        }

        sheetDto.UpdateEntity(sheetMusic);

        if (fileStream != null)
        {
            try
            {
                if (!string.IsNullOrEmpty(sheetMusic.SystemFileName))
                {
                    await fileStorageService.DeleteFile(sheetMusic.SystemFileName, "update");
                }
            }
            catch (FileNotFoundException)
            {
                // File has somehow been lost, local state is corrupt, but we proceed with update because:
                // 1. This could be user error, by manually adjusting files
                // 2. Crashing here would make this entry not updatable unless a file is produced.
                // It's better to just allow user to insert a new file.
            }

            sheetMusic.FileName = sheetMusic.GetFileName();
            sheetMusic.SystemFileName = sheetMusic.GetSystemFileName();
            await fileStorageService.SaveFileAsync(fileStream, sheetMusic.SystemFileName);
        }
        else
        {
            var oldFileName = sheetMusic.SystemFileName;

            if (oldFileName != null)
            {
                sheetMusic.FileName = sheetMusic.GetFileName();
                sheetMusic.SystemFileName = sheetMusic.GetSystemFileName();
                fileStorageService.RenameFile(oldFileName, sheetMusic.SystemFileName);
            }
            else
            {
                throw new InvalidOperationException("Existing file not found for renaming");
            }
        }

        unitOfWork.SheetMusic.Update(sheetMusic);
        await unitOfWork.SaveChangesAsync();

        return SheetMusicDto.FromEntity(sheetMusic);
    }

    public async Task DeleteSheetMusicAsync(uint id)
    {
        var sheetMusic = await unitOfWork.SheetMusic.GetByIdAsync(id);

        if (sheetMusic == null)
        {
            throw new InvalidOperationException("SheetMusic not found");
        }

        try
        {
            if (!string.IsNullOrEmpty(sheetMusic.SystemFileName))
            {
                await fileStorageService.DeleteFile(sheetMusic.SystemFileName, "delete");
            }
        }
        catch (FileNotFoundException)
        {
            // File has somehow been lost, local state is corrupt, but we proceed with deletion because:
            // 1. This could be user error, by manually adjusting files
            // 2. Crashing here would make this entry undeletable unless a file is produced.
        }

        unitOfWork.SheetMusic.Remove(sheetMusic);
        await unitOfWork.SaveChangesAsync();
    }

    public bool HasValidFile(SheetMusicDto sheetDto)
    {
        if (string.IsNullOrEmpty(sheetDto.SystemFileName) || string.IsNullOrWhiteSpace(sheetDto.FileName))
        {
            return false;
        }

        return fileStorageService.FileExists(sheetDto.SystemFileName);
    }
}