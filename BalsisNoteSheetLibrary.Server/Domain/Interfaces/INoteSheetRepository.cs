using BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Server.Domain.Interfaces;

public interface INoteSheetRepository
{
    Task<IReadOnlyList<NoteSheet>> GetAllOrderedByTitleAsync(CancellationToken cancellationToken = default);
}
