using System.Data;
using BalsisSheetMusicLibrary.Server.Domain.Entities;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.DbContext.Configurations;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.Extensions;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BalsisSheetMusicLibrary.Server.Infrastructure.Data.DbContext;

public class AppDbContext : IdentityDbContext, IDataProtectionKeyContext
{
    private bool _collationRegistered = false;
    
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
        if (sqliteConnection.State != ConnectionState.Open || _collationRegistered)
        {
            return;
        }

        try
        {
            SqliteExtensions.RegisterCaseInsensitiveCollation(sqliteConnection);
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
        {
            // Collation already exists on this connection, ignore.
        }
        finally
        {
            _collationRegistered = true;
        }
    }

    public DbSet<SheetMusic> SheetMusic { get; set; } = null!;
    public DbSet<SetList> SetLists { get; set; } = null!;
    public DbSet<SetListItem> SetListItems { get; set; } = null!;
    
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

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
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1)
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