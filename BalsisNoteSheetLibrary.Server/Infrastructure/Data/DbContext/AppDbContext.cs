using System.Data;
using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext.Configurations;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.Extensions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        var connection = Database.GetDbConnection();

        if (connection is not SqliteConnection sqliteConnection)
        {
            return;
        }

        sqliteConnection.StateChange += OnSqliteConnectionStateChange;

        // In case the connection is already open (e.g., from a pool)
        // when the DbContext is created, ensure collation is set.
        // The StateChange event might not fire if it's already open.
        // However, CreateCollation throws if already defined on the connection.
        // Relying on StateChange from Closed to Open is generally safer.
        // If issues persist with pooled connections, this part might need refinement
        // to check if collation is already defined before attempting to create.
        if (sqliteConnection.State != ConnectionState.Open)
        {
            return;
        }

        try
        {
            SqliteExtensions.RegisterCaseInsensitiveCollation(sqliteConnection);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("collation") &&
                                         ex.Message.Contains("already exists"))
        {
            // Collation already exists on this connection, ignore.
        }
    }

    public DbSet<NoteSheet> NoteSheets { get; set; } = null!;
    public DbSet<SetList> SetLists { get; set; } = null!;
    public DbSet<SetListItem> SetListItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new SetListItemConfiguration());
    }

    private static void OnSqliteConnectionStateChange(object? sender, StateChangeEventArgs e)
    {
        if (e.CurrentState != ConnectionState.Open || sender is not SqliteConnection connection)
        {
            return;
        }

        try
        {
            SqliteExtensions.RegisterCaseInsensitiveCollation(connection);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("collation") &&
                                         ex.Message.Contains("already exists"))
        {
            // Collation already exists on this connection (e.g. if event fired multiple times for same open state), ignore.
        }
    }

    public override void Dispose()
    {
        var connection = Database.GetDbConnection();

        if (connection is SqliteConnection sqliteConnection)
        {
            sqliteConnection.StateChange -= OnSqliteConnectionStateChange;
        }

        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        var connection = Database.GetDbConnection();

        if (connection is SqliteConnection sqliteConnection)
        {
            sqliteConnection.StateChange -= OnSqliteConnectionStateChange;
        }

        await base.DisposeAsync();
    }
}