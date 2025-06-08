using BalsisNoteSheetLibrary.Server.Helpers;
using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class NoteSheetController(AppDbContext context) : ControllerBase
{
    public async Task<AppResponse<IEnumerable<NoteSheet>>> GetAll()
    {
        var sheets = await context.NoteSheets.OrderBy(sheet =>
            EF.Functions.Collate(sheet.Title, SqliteExtensions.InsensitiveCollation))
            .ToArrayAsync();

        return new AppResponse<IEnumerable<NoteSheet>>(sheets, true);
    }

    [HttpGet("{id:int}")]
    public async Task<AppResponse<NoteSheet?>> Get(uint id)
    {
        var sheet = await context.NoteSheets.FindAsync(id);

        return new AppResponse<NoteSheet?>(
            sheet,
            sheet is not null,
            sheet is null ? "Note sheet not found" : string.Empty
        );
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<AppResponse<string>> Add(NoteSheet noteSheet)
    {
        context.NoteSheets.Add(noteSheet);
        await context.SaveChangesAsync();

        return new AppResponse<string>("Note sheet added", true);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<AppResponse<string>> Update(NoteSheet noteSheet)
    {
        var sheet = await context.NoteSheets.FindAsync(noteSheet.Id);

        if (sheet is null)
        {
            return new AppResponse<string>(null, false, "Note sheet not found");
        }

        context.Entry(sheet).CurrentValues.SetValues(noteSheet);
        await context.SaveChangesAsync();

        return new AppResponse<string>("Note sheet updated", true);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<AppResponse<string>> Delete(uint id)
    {
        var sheet = await context.NoteSheets.FindAsync(id);

        if (sheet is null)
        {
            return new AppResponse<string>(null, false, "Note sheet not found");
        }

        context.NoteSheets.Remove(sheet);
        await context.SaveChangesAsync();

        return new AppResponse<string>("Note sheet deleted", true);
    }
}

