namespace BalsisNoteSheetLibrary.Server.Domain.Interfaces;

public interface IUnitOfWork
{
    INoteSheetRepository NoteSheets { get; }
    ISetListRepository SetLists { get; }
    ISetListItemRepository SetListItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}