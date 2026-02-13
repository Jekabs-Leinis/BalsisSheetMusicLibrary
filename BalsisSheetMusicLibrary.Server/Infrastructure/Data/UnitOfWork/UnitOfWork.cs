using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.DbContext;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.Repositories;

namespace BalsisSheetMusicLibrary.Server.Infrastructure.Data.UnitOfWork;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public INoteSheetRepository NoteSheets { get; } = new NoteSheetRepository(dbContext);
    public ISetListRepository SetLists { get; } = new SetListRepository(dbContext);
    public ISetListItemRepository SetListItems { get; } = new SetListItemRepository(dbContext);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}