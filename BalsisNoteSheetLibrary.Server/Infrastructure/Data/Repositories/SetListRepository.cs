using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;

public class SetListRepository(AppDbContext context) : BaseRepository<SetList>(context), ISetListRepository
{
    public new async Task<SetList?> GetByIdAsync(uint id)
    {
        return await DbContext.SetLists
            .Include(list => list.Items)
            .FirstOrDefaultAsync(list => list.Id == id);
    }

    public new async Task<List<SetList>> GetAllAsync()
    {
        return await DbContext.SetLists
            .Include(list => list.Items)
            .Where(list => list.ArchivedAt == null)
            .OrderBy(list => list.Order)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SetList>> GetAllWithNoteSheetsAsync()
    {
        return await DbContext.SetLists
            .Include(list => list.Items)
            .ThenInclude(item => item.NoteSheet)
            .Where(list => list.ArchivedAt == null)
            .OrderBy(list => list.Order)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SetList>> GetAllArchivedAsync()
    {
        return await DbContext.SetLists
            .Include(list => list.Items)
            .ThenInclude(item => item.NoteSheet)
            .Where(list => list.ArchivedAt != null)
            .OrderBy(list => list.ArchivedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<SetList>> GetAllWithTrackingAsync()
    {
        return await DbContext.SetLists
            .Where(list => list.ArchivedAt == null)
            .OrderBy(sl => sl.Order)
            .ToListAsync();
    }

    public async Task<uint> GetMaxOrderAsync()
    {
        return await DbContext.SetLists.Where(sl => sl.Order != null).MaxAsync(sl => sl.Order) ?? 0;
    }
}