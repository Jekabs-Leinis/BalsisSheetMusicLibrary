using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;

public class NoteSheetRepository(AppDbContext context) : INoteSheetRepository
{
    public async Task<IReadOnlyList<NoteSheet>> GetAllOrderedByTitleAsync(CancellationToken cancellationToken = default)
    {
        return await context.Set<NoteSheet>()
            .AsNoTracking()
            .OrderBy(sheet => EF.Functions.Collate(sheet.Title, SqliteExtensions.InsensitiveCollation))
            .ToListAsync(cancellationToken);
    }
}