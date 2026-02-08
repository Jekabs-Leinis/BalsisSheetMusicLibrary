using BalsisNoteSheetLibrary.Server.Application.DTOs.NoteSheet;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Application.Services;

public class NoteSheetService(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService)
    : INoteSheetService
{
    public async Task<NoteSheetDto?> GetNoteSheetAsync(uint id)
    {
        var noteSheet = await unitOfWork.NoteSheets.GetByIdAsync(id);

        return noteSheet != null ? NoteSheetDto.FromEntity(noteSheet) : null;
    }

    public async Task<List<NoteSheetDto>> GetAllNoteSheetsAsync()
    {
        var noteSheets = await unitOfWork.NoteSheets.GetAsync(
            orderBy: sheet => EF.Functions.Collate(sheet.Title, SqliteExtensions.InsensitiveCollation)!
        );

        return noteSheets.Select(NoteSheetDto.FromEntity).ToList();
    }

    public async Task<NoteSheetDto> CreateNoteSheetAsync(CreateNoteSheetDto dto, Stream fileStream)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        
        var noteSheet = dto.ToEntity();
        unitOfWork.NoteSheets.Add(noteSheet);
        // Save initially to generate ID for filename
        await unitOfWork.SaveChangesAsync();

        noteSheet.FileName = noteSheet.GetFileName();
        noteSheet.SystemFileName = noteSheet.GetSystemFileName();
        
        try
        {
            await fileStorageService.SaveFileAsync(fileStream, noteSheet.SystemFileName);
            await unitOfWork.SaveChangesAsync();
        }
        catch
        {
            // Rollback: delete the file if it was created
            if (!string.IsNullOrEmpty(noteSheet.SystemFileName))
            {
                await fileStorageService.DeleteFile(noteSheet.SystemFileName);
            }
            // Remove the entity from tracking
            unitOfWork.NoteSheets.Remove(noteSheet);
            await unitOfWork.SaveChangesAsync();
            
            throw;
        }

        return NoteSheetDto.FromEntity(noteSheet);
    }

    public async Task<NoteSheetDto> UpdateNoteSheetAsync(UpdateNoteSheetDto dto, Stream? fileStream)
    {
        var noteSheet = await unitOfWork.NoteSheets.GetByIdAsync(dto.Id);

        if (noteSheet == null)
        {
            throw new InvalidOperationException("NoteSheet not found");
        }

        dto.UpdateEntity(noteSheet);

        if (fileStream != null)
        {
            if (!string.IsNullOrEmpty(noteSheet.SystemFileName))
            {
                await fileStorageService.DeleteFile(noteSheet.SystemFileName);
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
                fileStorageService.RenameFile(oldFileName, noteSheet.SystemFileName);
            }
            else
            {
                throw new InvalidOperationException("Existing file not found for renaming");
            }
        }

        unitOfWork.NoteSheets.Update(noteSheet);
        await unitOfWork.SaveChangesAsync();

        return NoteSheetDto.FromEntity(noteSheet);
    }

    public async Task DeleteNoteSheetAsync(uint id)
    {
        var noteSheet = await unitOfWork.NoteSheets.GetByIdAsync(id);

        if (noteSheet == null)
        {
            throw new InvalidOperationException("NoteSheet not found");
        }

        if (!string.IsNullOrEmpty(noteSheet.SystemFileName))
        {
            await fileStorageService.DeleteFile(noteSheet.SystemFileName);
        }

        unitOfWork.NoteSheets.Remove(noteSheet);
        await unitOfWork.SaveChangesAsync();
    }

    public bool HasValidFile(NoteSheetDto dto)
    {
        if (string.IsNullOrEmpty(dto.SystemFileName) || string.IsNullOrWhiteSpace(dto.FileName))
        {
            return false;
        }

        return fileStorageService.FileExists(dto.SystemFileName);
    }
}