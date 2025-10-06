using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;

public class SetListRepository(AppDbContext context) : ISetListRepository
{
    public async Task<SetList?> GetByIdAsync(uint id)
    {
        return await context.SetLists
            .Include(list => list.Items)
            .FirstOrDefaultAsync(list => list.Id == id);
    }

    public async Task<List<SetList>> GetAllAsync()
    {
        return await context.SetLists
            .Include(list => list.Items)
            .Where(list => list.ArchivedAt == null)
            .OrderBy(list => list.Order)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SetList>> GetAllWithNoteSheetsAsync()
    {
        return await context.SetLists
            .Include(list => list.Items)
            .ThenInclude(item => item.NoteSheet)
            .Where(list => list.ArchivedAt == null)
            .OrderBy(list => list.Order)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SetList>> GetAllArchivedAsync()
    {
        return await context.SetLists
            .Include(list => list.Items)
            .ThenInclude(item => item.NoteSheet)
            .Where(list => list.ArchivedAt != null)
            .OrderBy(list => list.ArchivedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SetList>> GetAllWithTrackingAsync()
    {
        return await context.SetLists
            .Where(list => list.ArchivedAt == null)
            .OrderBy(sl => sl.Order)
            .ToListAsync();
    }
}