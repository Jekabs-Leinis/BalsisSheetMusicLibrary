using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.Entities;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;

namespace BalsisNoteSheetLibrary.Server.Application.Services;

public class SetListService(IUnitOfWork unitOfWork) : ISetListService
{
    public async Task<IEnumerable<SetListDto>> GetAllSetListsAsync(bool withNoteSheets = false)
    {
        IEnumerable<SetList> setLists;

        if (withNoteSheets)
        {
            setLists = await unitOfWork.SetLists.GetAsync(list => list.ArchivedAt == null, list => list.Order,
                includeProperties: ["Items.NoteSheet"], withTracking: false);
        }
        else
        {
            setLists = await unitOfWork.SetLists.GetAsync(list => list.ArchivedAt == null, list => list.Order,
                includeProperties: ["Items"], withTracking: false);
        }

        return setLists.Select(SetListDto.FromEntity);
    }

    public async Task<IEnumerable<SetListDto>> GetAllArchivedSetListsAsync()
    {
        var setLists = await unitOfWork.SetLists.GetAsync(list => list.ArchivedAt != null, list => list.ArchivedAt,
            includeProperties: ["Items.NoteSheet"], withTracking: false);

        return setLists.Select(SetListDto.FromEntity);
    }

    public async Task<SetListDto?> GetSetListByIdAsync(uint id)
    {
        var setList = await unitOfWork.SetLists.GetByIdWithItemsAsync(id);

        return setList == null ? null : SetListDto.FromEntity(setList);
    }

    public async Task<SetListDto> CreateSetListAsync(CreateSetListDto dto)
    {
        var setList = dto.ToEntity();

        var maxOrder = await unitOfWork.SetLists.GetMaxOrderAsync();
        setList.Order = maxOrder + 1;

        unitOfWork.SetLists.Add(setList);
        await unitOfWork.SaveChangesAsync();

        return SetListDto.FromEntity(setList);
    }

    public async Task<SetListDto> UpdateSetListAsync(UpdateSetListDto dto)
    {
        var setList = await unitOfWork.SetLists.GetByIdWithItemsAsync(dto.Id);

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
            unitOfWork.SetListItems.RemoveRange(itemsToRemove);
        }

        // Update existing items and add new ones
        // Have to use a for loop to reorder the remaining and new items
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

                unitOfWork.SetListItems.Update(existingItem);
            }
            else
            {
                // New item
                updatedItem.SetListId = setList.Id;
                updatedItem.Order = (uint)i;
                unitOfWork.SetListItems.Add(updatedItem);
            }
        }

        unitOfWork.SetLists.Update(setList);
        await unitOfWork.SaveChangesAsync();

        return SetListDto.FromEntity(setList);
    }

    public async Task DeleteSetListAsync(uint id)
    {
        var setList = await unitOfWork.SetLists.GetByIdWithItemsAsync(id);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        unitOfWork.SetLists.Remove(setList);
        unitOfWork.SetListItems.RemoveRange(setList.Items);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task MoveSetListAsync(MoveSetListDto dto)
    {
        var setList = await unitOfWork.SetLists.GetByIdAsync(dto.Id);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        // Have to reorder all set lists to ensure no gaps or duplicates
        // Otherwise updating from two different clients without either reloading causes issues
        var reorderableLists = await unitOfWork.SetLists.GetAsync(list => list.ArchivedAt == null, list => list.Order);
        reorderableLists.RemoveAll(sl => sl.Id == setList.Id);

        if (reorderableLists.Count < dto.NewOrder)
        {
            throw new InvalidOperationException("New order is out of bounds");
        }

        reorderableLists.Insert((int)dto.NewOrder, setList);
        ReorderSetLists(reorderableLists);
        unitOfWork.SetLists.UpdateRange(reorderableLists);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task ArchiveSetListAsync(uint id)
    {
        var setList = await unitOfWork.SetLists.GetByIdAsync(id);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        setList.ArchivedAt = DateTime.UtcNow;
        setList.Order = null;
        unitOfWork.SetLists.Update(setList);

        var reorderableLists = await unitOfWork.SetLists.GetAsync(list => list.ArchivedAt == null, list => list.Order);
        reorderableLists.RemoveAll(sl => sl.Id == setList.Id);

        ReorderSetLists(reorderableLists);
        unitOfWork.SetLists.UpdateRange(reorderableLists);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreSetListAsync(uint id)
    {
        var setList = await unitOfWork.SetLists.GetByIdAsync(id);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        setList.ArchivedAt = null;

        var reorderableLists = await unitOfWork.SetLists.GetAsync(list => list.ArchivedAt == null, list => list.Order);
        reorderableLists.Add(setList);

        ReorderSetLists(reorderableLists);
        unitOfWork.SetLists.UpdateRange(reorderableLists);

        await unitOfWork.SaveChangesAsync();
    }

    private static void ReorderSetLists(List<SetList> setLists)
    {
        for (var i = 0; i < setLists.Count; i++) setLists[i].Order = (uint)i;
    }
}