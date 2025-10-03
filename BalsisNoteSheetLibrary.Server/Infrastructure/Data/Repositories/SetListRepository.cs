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

    public async Task<IEnumerable<SetList>> GetAllAsync()
    {
        return await context.SetLists
            .Include(s => s.Items)
            .Where(list => list.ArchivedAt == null)
            .OrderBy(list => list.Order)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<SetList>> GetAllArchivedAsync()
    {
        return await context.SetLists
            .Include(list => list.Items)
            .Where(list => list.ArchivedAt != null)
            .OrderBy(list => list.ArchivedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}