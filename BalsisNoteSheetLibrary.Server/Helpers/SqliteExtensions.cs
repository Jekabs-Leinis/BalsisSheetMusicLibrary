using System.Globalization;
using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Helpers;

public abstract class SqliteExtensions
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
        
        Console.WriteLine("Fold collation created for SQLite database.");
    }
}

