using BalsisSheetMusicLibrary.Server.Domain.Entities;
using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.DbContext;

namespace BalsisSheetMusicLibrary.Server.Infrastructure.Data.Repositories;

public class NoteSheetRepository(AppDbContext context) : BaseRepository<NoteSheet>(context), INoteSheetRepository
{
}