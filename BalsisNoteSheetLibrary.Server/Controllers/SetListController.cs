using BalsisNoteSheetLibrary.Server.DTOs;
using BalsisNoteSheetLibrary.Server.Helpers;
using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Controllers;

[ApiController]
[Route("api/[controller]/[action]", Name = "[controller]_[action]")]
[Authorize(Roles = $"{Role.Admin},{Role.User}")]
public class SetListController(AppDbContext context) : ControllerBase
{
    [HttpGet(Name = "GetAll")]
    public async Task<BaseResponseDto<IEnumerable<SetListDto>>> GetAll(bool withSheets = false,
        bool withArchived = false)
    {
        var query = context.SetLists
            .If(!withArchived, q => q.Where(sl => sl.ArchivedAt == null))
            .Include(list => list.Items)
            .ThenInclude(item => item.NoteSheet)
            .OrderBy(list => list.Order);

        var setLists = await query.ToListAsync();
        var result = setLists.Select(sl => SetListDto.FromEntity(sl, withSheets)).ToList();

        return new BaseResponseDto<IEnumerable<SetListDto>>(result);
    }

    [HttpGet("{id:int}")]
    public async Task<BaseResponseDto<SetListDto?>> Get(uint id, bool withSheets = false)
    {
        var setList = await context.SetLists
            .Include(sl => sl.Items)
            .ThenInclude(i => i.NoteSheet)
            .FirstOrDefaultAsync(sl => sl.Id == id);

        if (setList == null)
        {
            return new BaseResponseDto<SetListDto?>(null, false, "Set list not found");
        }

        var result = SetListDto.FromEntity(setList, withSheets);
        return new BaseResponseDto<SetListDto?>(result);
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<BaseResponseDto<string>> Add([FromForm] SetListDto setListDto)
    {
        var setList = new SetList
        {
            Title = setListDto.Title,
            Order = setListDto.Order
        };

        context.SetLists.Add(setList);
        await context.SaveChangesAsync();

        if (setListDto.Items.Any())
        {
            foreach (var itemDto in setListDto.Items)
            {
                context.SetListItems.Add(new SetListItem
                {
                    SetListId = setList.Id,
                    NoteSheetId = itemDto.NoteSheetId,
                    Order = itemDto.Order
                });
            }

            await context.SaveChangesAsync();
        }

        return new BaseResponseDto<string>("Set list added");
    }

    [HttpPost]
    [Authorize(Roles = Role.Admin)]
    public async Task<BaseResponseDto> Update([FromBody] SetListDto setListDto)
    {
        var existingSetList = await context.SetLists
            .Include(sl => sl.Items)
            .FirstOrDefaultAsync(sl => sl.Id == setListDto.Id);

        if (existingSetList is null)
        {
            return new BaseResponseDto("Set list not found", false);
        }

        // Update main properties
        existingSetList.Title = setListDto.Title;
        existingSetList.Order = setListDto.Order;


        // Update Items
        // Remove items not present in the incoming setList
        var incomingItemIds = setListDto.Items.Select(i => i.NoteSheetId).ToHashSet();
        var itemsToRemove = existingSetList.Items
            .Where(i => i.NoteSheetId.HasValue && !incomingItemIds.Contains(i.NoteSheetId.Value))
            .ToList();

        foreach (var item in itemsToRemove)
        {
            context.SetListItems.Remove(item);
        }

        // Update existing and add new items
        foreach (var itemDto in setListDto.Items)
        {
            var existingItem = existingSetList.Items
                .FirstOrDefault(i => i.NoteSheetId == itemDto.NoteSheetId);

            if (existingItem != null)
            {
                existingItem.Order = itemDto.Order;
            }
            else if (itemDto.NoteSheetId.HasValue)
            {
                context.SetListItems.Add(new SetListItem
                {
                    SetListId = existingSetList.Id,
                    NoteSheetId = itemDto.NoteSheetId,
                    Order = itemDto.Order
                });
            }
        }

        await context.SaveChangesAsync();

        return new BaseResponseDto("Set list updated");
    }

    public async Task<BaseResponseDto> UpdateOrder([FromForm] SetListDto setListDto)
    {
        var existingSetList = await context.SetLists
            .FirstOrDefaultAsync(sl => sl.Id == setListDto.Id);

        if (existingSetList is null)
        {
            return new BaseResponseDto("Set list not found", false);
        }
        
        existingSetList.Order = setListDto.Order;
        
        await context.SaveChangesAsync();

        return new BaseResponseDto("Set list order updated");
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<BaseResponseDto> Delete(uint id)
    {
        var setList = await context.SetLists.FindAsync(id);

        if (setList is null)
        {
            return new BaseResponseDto("Set list not found", false);
        }
        
        if (setList.Order.HasValue)
            await context.SetLists.Where(sl => sl.Order > setList.Order)
                .ForEachAsync(sl => sl.Order -= 1);

        context.SetLists.Remove(setList);

        // Also remove associated items
        var itemsToRemove = context.SetListItems.Where(item => item.SetListId == id);
        context.SetListItems.RemoveRange(itemsToRemove);

        await context.SaveChangesAsync();

        return new BaseResponseDto("Set list deleted");
    }

    [HttpPost("{id:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<BaseResponseDto> Archive(uint id)
    {
        var setList = await context.SetLists.FindAsync(id);

        if (setList is null)
        {
            return new BaseResponseDto("Set list not found", false);
        }

        setList.ArchivedAt = DateTime.UtcNow;
        setList.Order = null;
        
        await context.SetLists.Where(sl => sl.Order > setList.Order)
            .ForEachAsync(sl => sl.Order -= 1);
        
        await context.SaveChangesAsync();

        return new BaseResponseDto("Set list archived");
    }

    [HttpPost("{id:int}")]
    [Authorize(Roles = Role.Admin)]
    public async Task<BaseResponseDto> Unarchive(uint id)
    {
        var setList = await context.SetLists.FindAsync(id);

        if (setList is null)
        {
            return new BaseResponseDto("Set list not found", false);
        }

        setList.ArchivedAt = null;
        setList.Order = (uint?)context.SetLists.Count(sl => sl.Order.HasValue);
        
        await context.SaveChangesAsync();

        return new BaseResponseDto("Set list unarchived");
    }
}