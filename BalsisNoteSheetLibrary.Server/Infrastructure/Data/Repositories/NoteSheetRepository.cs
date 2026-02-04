using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;

public class NoteSheetRepository(AppDbContext context) : BaseRepository<NoteSheet>(context), INoteSheetRepository
{
}