using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;

public class SetListItemRepository(AppDbContext context) : BaseRepository<SetListItem>(context), ISetListItemRepository
{
    
}