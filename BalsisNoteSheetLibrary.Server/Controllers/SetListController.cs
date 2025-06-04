using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
public class SetListController(AppDbContext context) : ControllerBase
{
    [HttpGet(Name = "GetAll")]
    public async Task<AppResponse<IEnumerable<SetList>>> GetAll()
    {
        var setLists = await context.SetLists
            .Include(list => list.Items)
            .OrderBy(list => list.Order)
            .ToListAsync();

        return new AppResponse<IEnumerable<SetList>>(setLists, true);
    }

    [HttpGet("{id:int}")]
    public async Task<AppResponse<SetList?>> Get(uint id)
    {
        var setList = await context.SetLists.FindAsync(id);

        return new AppResponse<SetList?>(
            setList,
            setList is not null,
            setList is null ? "Set list not found" : string.Empty
        );
    }

    [HttpPost]
    public async Task<AppResponse<string>> Add(SetList setList)
    {
        context.SetLists.Add(setList);
        await context.SaveChangesAsync();

        return new AppResponse<string>("Set list added", true);
    }

    [HttpPost]
    public async Task<AppResponse<string>> Update(SetList setList)
    {
        var existingSetList = await context.SetLists
            .Include(sl => sl.Items)
            .FirstOrDefaultAsync(sl => sl.Id == setList.Id);

        if (existingSetList is null)
        {
            return new AppResponse<string>(null, false, "Set list not found");
        }

        context.Entry(existingSetList).CurrentValues.SetValues(setList);

        // Update Items
        // Remove items not present in the incoming setList
        var incomingItemIds = setList.Items.Select(i => i.NoteSheetId).ToHashSet();
        var itemsToRemove = existingSetList.Items.Where(i => !incomingItemIds.Contains(i.NoteSheetId)).ToList();
        foreach (var item in itemsToRemove)
        {
            context.Remove(item);
        }

        // Update existing and add new items
        foreach (var item in setList.Items)
        {
            var existingItem = existingSetList.Items.FirstOrDefault(i => i.NoteSheetId == item.NoteSheetId);
            if (existingItem != null)
            {
                context.Entry(existingItem).CurrentValues.SetValues(item);
            }
            else
            {
                context.SetListItems.Add(item);
            }
        }

        await context.SaveChangesAsync();

        return new AppResponse<string>("Set list updated", true);
    }

    [HttpDelete("{id:int}")]
    public async Task<AppResponse<string>> Delete(uint id)
    {
        var setList = await context.SetLists.FindAsync(id);

        if (setList is null)
        {
            return new AppResponse<string>(null, false, "Set list not found");
        }

        context.SetLists.Remove(setList);
        await context.SaveChangesAsync();

        return new AppResponse<string>("Set list deleted", true);
    }
}
