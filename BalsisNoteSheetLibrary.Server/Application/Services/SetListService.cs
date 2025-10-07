using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Application.Services;

public class SetListService(AppDbContext context, ISetListRepository setListRepository) : ISetListService
{
    public async Task<IEnumerable<SetListDto>> GetAllSetListsAsync(bool withNoteSheets = false)
    {
        IEnumerable<SetList> setLists;

        if (withNoteSheets)
        {
            setLists = await setListRepository.GetAllWithNoteSheetsAsync();
        }
        else
        {
            setLists = await setListRepository.GetAllAsync();
        }

        return setLists.Select(SetListDto.FromEntity);
    }

    public async Task<IEnumerable<SetListDto>> GetAllArchivedSetListsAsync()
    {
        var setLists = await setListRepository.GetAllArchivedAsync();

        return setLists.Select(SetListDto.FromEntity);
    }

    public async Task<SetListDto?> GetSetListByIdAsync(uint id)
    {
        var setList = await setListRepository.GetByIdAsync(id);

        return setList == null ? null : SetListDto.FromEntity(setList);
    }

    public async Task<SetListDto> CreateSetListAsync(CreateSetListDto dto)
    {
        var setList = dto.ToEntity();

        var maxOrder = await context.SetLists.Where(sl => sl.Order != null).MaxAsync(sl => sl.Order) ?? 0;
        setList.Order = maxOrder + 1;

        context.SetLists.Add(setList);
        await context.SaveChangesAsync();

        return SetListDto.FromEntity(setList);
    }

    public async Task<SetListDto> UpdateSetListAsync(UpdateSetListDto dto)
    {
        var setList = await setListRepository.GetByIdAsync(dto.Id);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        setList.Title = dto.Title;

        var existingItems = setList.Items.ToList();
        var updatedItems = dto.Items.Select(i => i.ToEntity()).ToList();

        // Find items to remove (present in DB, not in DTO)
        var itemsToRemove = existingItems.Where(ei => updatedItems.All(ui => ui.NoteSheetId != ei.NoteSheetId))
            .ToList();

        if (itemsToRemove.Count > 0)
        {
            context.SetListItems.RemoveRange(itemsToRemove);
        }

        // Update existing items and add new ones
        // Use a for loop to reorder the remaining and new items
        // We cannot rely on the Order property from the DTO,
        // as two clients editing at the same time could cause gaps or duplicates
        for (var i = 0; i < updatedItems.Count; i++)
        {
            var updatedItem = updatedItems[i];
            var existingItem = existingItems.FirstOrDefault(ei => ei.NoteSheetId == updatedItem.NoteSheetId);

            if (existingItem != null)
            {
                // Update properties if changed
                existingItem.SetListId = setList.Id;
                existingItem.NoteSheetId = updatedItem.NoteSheetId;
                existingItem.Order = (uint)i;

                context.SetListItems.Update(existingItem);
            }
            else
            {
                // New item
                updatedItem.SetListId = setList.Id;
                updatedItem.Order = (uint)i;
                context.SetListItems.Add(updatedItem);
            }
        }

        context.SetLists.Update(setList);
        await context.SaveChangesAsync();

        return SetListDto.FromEntity(setList);
    }

    public async Task DeleteSetListAsync(uint id)
    {
        var setList = await setListRepository.GetByIdAsync(id);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        context.SetLists.Remove(setList);
        context.SetListItems.RemoveRange(setList.Items);

        await context.SaveChangesAsync();
    }

    public async Task MoveSetListAsync(MoveSetListDto dto)
    {
        var setList = await setListRepository.GetByIdAsync(dto.Id);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        // Have to reorder all set lists to ensure no gaps or duplicates
        // Otherwise updating from two different clients without either reloading causes issues
        var reorderableLists = await setListRepository.GetAllWithTrackingAsync();
        reorderableLists.RemoveAll(sl => sl.Id == setList.Id);
        reorderableLists.Insert((int)dto.NewOrder, setList);
        ReorderSetLists(reorderableLists);
        context.UpdateRange(reorderableLists);

        await context.SaveChangesAsync();
    }

    public async Task ArchiveSetListAsync(uint id)
    {
        var setList = await setListRepository.GetByIdAsync(id);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        setList.ArchivedAt = DateTime.Now;
        setList.Order = null;
        context.SetLists.Update(setList);

        var reorderableLists = await setListRepository.GetAllWithTrackingAsync();
        reorderableLists.RemoveAll(sl => sl.Id == setList.Id);

        ReorderSetLists(reorderableLists);
        context.UpdateRange(reorderableLists);

        await context.SaveChangesAsync();
    }

    public async Task RestoreSetListAsync(uint id)
    {
        var setList = await setListRepository.GetByIdAsync(id);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        setList.ArchivedAt = null;

        var reorderableLists = await setListRepository.GetAllWithTrackingAsync();
        reorderableLists.Add(setList);

        ReorderSetLists(reorderableLists);
        context.UpdateRange(reorderableLists);

        await context.SaveChangesAsync();
    }

    private static void ReorderSetLists(List<SetList> setLists)
    {
        for (var i = 0; i < setLists.Count; i++) setLists[i].Order = (uint)i;
    }
}