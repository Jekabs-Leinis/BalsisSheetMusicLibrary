using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace BasisNoteSheetLibrary.Tests.Integration;

public abstract class IntegrationTestBase: IDisposable
{
    protected readonly IUnitOfWork UnitOfWork;
    
    protected readonly AppDbContext DbContext;

    protected IntegrationTestBase()
    {
        DbContext = GetDbContext().Result;
        UnitOfWork = new UnitOfWork(DbContext);
    }
    
    private static async Task<AppDbContext> GetDbContext()
    {
        // Set up options to use an in-memory SQLite database
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=file:inmem?mode=memory&cache=shared")
            .Options;
        
        var context = new AppDbContext(options);
        // Ensure the database is created and can be connected to
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        
        return context;
    }
    
    public void Dispose()
    {
        DbContext.Database.EnsureDeleted();
        DbContext.Dispose();
    }
}