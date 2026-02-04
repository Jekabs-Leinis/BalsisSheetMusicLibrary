using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;

public class SetListRepository(AppDbContext context) : BaseRepository<SetList>(context), ISetListRepository
{
    public async Task<uint> GetMaxOrderAsync()
    {
        return await DbSet.Where(sl => sl.Order != null).MaxAsync(sl => sl.Order) ?? 0;
    }
}