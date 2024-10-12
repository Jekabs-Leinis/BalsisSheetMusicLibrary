using BalsisNoteSheetLibrary.Server.Helpers;
using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
public class NoteSheetController(AppDbContext context) : AppControllerBase(context)
{
    public AppResponse<IEnumerable<NoteSheet>> GetAll()
    {
        SqliteExtensions.SetupInsensitiveCollation(Context);

        var sheets = Context.NoteSheets.OrderBy(sheet =>
            EF.Functions.Collate(sheet.Title, SqliteExtensions.InsensitiveCollation));

        return new AppResponse<IEnumerable<NoteSheet>>(sheets, true);
    }

    [HttpGet("{id:int}")]
    public AppResponse<NoteSheet?> Get(uint id)
    {
        var sheet = Context.NoteSheets.Find(id);

        return new AppResponse<NoteSheet?>(
            sheet,
            sheet is not null,
            sheet is null ? "Note sheet not found" : string.Empty
        );
    }

    [HttpPost]
    public AppResponse<string> Add(NoteSheet noteSheet)
    {
        Context.NoteSheets.Add(noteSheet);
        Context.SaveChanges();

        return new AppResponse<string>("Note sheet added", true);
    }

    [HttpPost]
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

    [HttpDelete("{id:int}")]
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