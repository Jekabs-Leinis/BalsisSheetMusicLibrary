using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.DbContext;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BalsisSheetMusicLibrary.Tests.Integration;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly AppDbContext DbContext;
    protected readonly IUnitOfWork UnitOfWork;

    protected IntegrationTestBase()
    {
        DbContext = GetDbContext().Result;
        UnitOfWork = new UnitOfWork(DbContext);
    }

    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }

    private static async Task<AppDbContext> GetDbContext()
    {
        // Set up options to use an in-memory SQLite database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=file:inmem?mode=memory")
            .Options;

        var context = new AppDbContext(options);
        // Ensure the database is created and can be connected to
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        return context;
    }
}