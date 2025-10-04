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
        var noteSheet = await context.NoteSheets.FindAsync(id);
        return noteSheet != null ? NoteSheetDto.FromEntity(noteSheet) : null;
    }

    public async Task<IEnumerable<NoteSheetDto>> GetAllNoteSheetsAsync()
    {
        var noteSheets = await repository.GetAllOrderedByTitleAsync();
        return noteSheets.Select(NoteSheetDto.FromEntity);
    }

    public async Task<NoteSheetDto> CreateNoteSheetAsync(CreateNoteSheetDto dto, Stream fileStream)
    {
        var noteSheet = dto.ToEntity();
        context.NoteSheets.Add(noteSheet);
        // Save initially to generate ID for filename
        await context.SaveChangesAsync();

        noteSheet.FileName = noteSheet.GetFileName();
        noteSheet.SystemFileName = noteSheet.GetFileName();
        await fileStorageService.SaveFileAsync(fileStream, noteSheet.SystemFileName);
        await context.SaveChangesAsync();

        return NoteSheetDto.FromEntity(noteSheet);
    }

    public async Task<NoteSheetDto> UpdateNoteSheetAsync(UpdateNoteSheetDto dto, Stream? fileStream)
    {
        var noteSheet = await context.NoteSheets.FindAsync(dto.Id);

        if (noteSheet == null)
        {
            throw new InvalidOperationException("NoteSheet not found");
        }

        dto.UpdateEntity(noteSheet);

        if (fileStream != null)
        {
            if (!string.IsNullOrEmpty(noteSheet.SystemFileName))
            {
                await fileStorageService.DeleteFileAsync(noteSheet.SystemFileName);
            }

            noteSheet.FileName = noteSheet.GetFileName();
            noteSheet.SystemFileName = noteSheet.GetSystemFileName();
            await fileStorageService.SaveFileAsync(fileStream, noteSheet.SystemFileName);
        }
        else
        {
            var oldFileName = noteSheet.SystemFileName;

            if (oldFileName != null)
            {
                noteSheet.FileName = noteSheet.GetFileName();
                noteSheet.SystemFileName = noteSheet.GetSystemFileName();
                fileStorageService.MoveFile(oldFileName, noteSheet.SystemFileName);
            }
            else
            {
                throw new InvalidOperationException("Existing file not found for renaming");
            }
        }

        await context.SaveChangesAsync();

        return NoteSheetDto.FromEntity(noteSheet);
    }

    public async Task DeleteNoteSheetAsync(uint id)
    {
        var noteSheet = await context.NoteSheets.FindAsync(id);

        if (noteSheet == null)
        {
            throw new InvalidOperationException("NoteSheet not found");
        }

        if (!string.IsNullOrEmpty(noteSheet.SystemFileName))
        {
            await fileStorageService.DeleteFileAsync(noteSheet.SystemFileName);
        }

        context.NoteSheets.Remove(noteSheet);
        await context.SaveChangesAsync();
    }
}