using System.Globalization;
using BalsisNoteSheetLibrary.Server.Helpers;
using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class NoteSheetController(AppDbContext context) : AppControllerBase(context)
{
    [HttpGet(Name = "GetAll")]
    public AppResponse<IEnumerable<NoteSheet>> GetAll()
    {
        var connection = Context.Database.GetDbConnection() as SqliteConnection;

        // By default SQLite does not support case-insensitive and diacritic-insensitive sorting
        connection!.CreateCollation("FOLD", (x, y) => string.Compare(
            StringExtensions.FoldToASCII(x),
            StringExtensions.FoldToASCII(y),
            CultureInfo.CurrentCulture,
            CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase
        ));

        var sheets = Context.NoteSheets.OrderBy(sheet => EF.Functions.Collate(sheet.Title, "FOLD"));

        return new AppResponse<IEnumerable<NoteSheet>>(sheets, true);
    }

    [HttpGet("{id:int}", Name = "Get")]
    public AppResponse<NoteSheet?> Get(uint id)
    {
        var sheet = Context.NoteSheets.Find(id);

        return sheet is not null
            ? new AppResponse<NoteSheet?>(sheet, true)
            : new AppResponse<NoteSheet?>(null, false, "Note sheet not found");
    }

    [HttpPost(Name = "Add")]
    public AppResponse<string> Add(NoteSheet noteSheet)
    {
        Context.NoteSheets.Add(noteSheet);
        Context.SaveChanges();

        return new AppResponse<string>("Note sheet added", true);
    }

    [HttpPost(Name = "Update")]
    public AppResponse<string> Update(NoteSheet noteSheet)
    {
        var sheet = Context.NoteSheets.Find(noteSheet.Id);

        if (sheet is null)
        {
            return new AppResponse<string>(null, false, "Note sheet not found");
        }

        Context.NoteSheets.Update(noteSheet);
        Context.SaveChanges();

        return new AppResponse<string>("Note sheet updated", true);
    }

    [HttpDelete("{id:int}", Name = "Delete")]
    public AppResponse<string> Delete(uint id)
    {
        var sheet = Context.NoteSheets.Find(id);

        if (sheet is null)
        {
            return new AppResponse<string>(null, false, "Note sheet not found");
        }

        Context.NoteSheets.Remove(sheet);
        Context.SaveChanges();

        return new AppResponse<string>("Note sheet deleted", true);
    }
}