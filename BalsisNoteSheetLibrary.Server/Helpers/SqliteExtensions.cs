using System.Globalization;
using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Helpers;

public abstract class SqliteExtensions
{
    public const string InsensitiveCollation = "FOLD";

    /*
     * Use this method when you need to sort SQLite data ignoring case and diacritics.
     */
    public static void SetupInsensitiveCollation(AppDbContext context)
    {
        var connection = context.Database.GetDbConnection() as SqliteConnection;

        // By default SQLite does not support case-insensitive and diacritic-insensitive sorting
        connection!.CreateCollation(InsensitiveCollation, (x, y) => string.Compare(
            StringExtensions.FoldToASCII(x),
            StringExtensions.FoldToASCII(y),
            CultureInfo.CurrentCulture,
            CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase
        ));
    }
}