using BalsisNoteSheetLibrary.Server.Application.DTOs;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services.Interfaces;

namespace BalsisNoteSheetLibrary.Server.Application.Services;

public class NoteSheetService(
    AppDbContext context,
    INoteSheetRepository repository,
    IFileStorageService fileStorageService)
    : INoteSheetService
{
    public async Task<NoteSheetDto?> GetNoteSheetAsync(uint id)
    {
        var entity = await context.NoteSheets.FindAsync(id);
        return entity != null ? NoteSheetDto.FromEntity(entity) : null;
    }

    public async Task<IEnumerable<NoteSheetDto>> GetAllNoteSheetsAsync()
    {
        var entities = await repository.GetAllOrderedByTitleAsync();
        return entities.Select(NoteSheetDto.FromEntity);
    }

    public async Task<NoteSheetDto> CreateNoteSheetAsync(CreateNoteSheetDto dto, Stream fileStream)
    {
        var entity = dto.ToEntity();
        context.NoteSheets.Add(entity);
        // Save initially to generate ID for filename
        await context.SaveChangesAsync();

        entity.FileName = fileStorageService.GetFileName(entity);
        entity.SystemFileName = fileStorageService.GetFileName(entity);
        await fileStorageService.SaveFileAsync(fileStream, entity.SystemFileName);
        await context.SaveChangesAsync();

        return NoteSheetDto.FromEntity(entity);
    }

    public async Task<NoteSheetDto> UpdateNoteSheetAsync(UpdateNoteSheetDto dto, Stream? fileStream)
    {
        var entity = await context.NoteSheets.FindAsync(dto.Id);

        if (entity == null)
        {
            throw new InvalidOperationException("NoteSheet not found");
        }

        dto.UpdateEntity(entity);

        if (fileStream != null)
        {
            if (!string.IsNullOrEmpty(entity.SystemFileName))
            {
                await fileStorageService.DeleteFileAsync(entity.SystemFileName);
            }

            entity.FileName = fileStorageService.GetFileName(entity);
            entity.SystemFileName = fileStorageService.GetSystemFileName(entity);
            await fileStorageService.SaveFileAsync(fileStream, entity.SystemFileName);
        }
        else
        {
            var oldFileName = entity.SystemFileName;

            if (oldFileName != null)
            {
                entity.FileName = fileStorageService.GetFileName(entity);
                entity.SystemFileName = fileStorageService.GetSystemFileName(entity);
                fileStorageService.MoveFile(oldFileName, entity.SystemFileName);
            }
            else
            {
                throw new InvalidOperationException("Existing file not found for renaming");
            }
        }

        await context.SaveChangesAsync();

        return NoteSheetDto.FromEntity(entity);
    }

    public async Task DeleteNoteSheetAsync(uint id)
    {
        var entity = await context.NoteSheets.FindAsync(id);

        if (entity == null)
        {
            throw new InvalidOperationException("NoteSheet not found");
        }

        if (!string.IsNullOrEmpty(entity.SystemFileName))
        {
            await fileStorageService.DeleteFileAsync(entity.SystemFileName);
        }

        context.NoteSheets.Remove(entity);
        await context.SaveChangesAsync();
    }
}