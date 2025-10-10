using BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Server.Domain.Interfaces;

public interface INoteSheetRepository : IBaseRepository<NoteSheet>
{
    Task<List<NoteSheet>> GetAllOrderedByTitleAsync();
}