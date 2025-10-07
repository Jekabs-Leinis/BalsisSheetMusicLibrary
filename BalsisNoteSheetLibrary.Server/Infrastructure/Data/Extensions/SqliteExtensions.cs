using System.Globalization;
using BalsisNoteSheetLibrary.Server.Application.Extensions;
using Microsoft.Data.Sqlite;
using Serilog;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Extensions;

public abstract class SqliteExtensions(ILogger<SqliteExtensions> logger)
{
    public const string InsensitiveCollation = "FOLD";

    /*
     * By default, SQLite does not support case-insensitive and diacritic-insensitive sorting
     */
    public static void RegisterCaseInsensitiveCollation(SqliteConnection connection)
    {
        connection!.CreateCollation(InsensitiveCollation, (x, y) => string.Compare(
            x.FoldToASCII(),
            y.FoldToASCII(),
            CultureInfo.CurrentCulture,
            CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase
        ));

        Log.Information("Fold collation created for SQLite database.");
    }
}