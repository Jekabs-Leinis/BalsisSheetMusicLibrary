using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;

namespace BalsisNoteSheetLibrary.Server.Application.Services;

public class SetListItemService(IUnitOfWork unitOfWork) : ISetListItemService
{
    public async Task MoveSetListItemAsync(MoveSetListItemDto dto)
    {
        var setLists = await unitOfWork.SetLists.GetAsync(sl => sl.Id == dto.SetListId, includeProperties: ["Items"]);

        if (setLists.Count == 0)
        {
            throw new InvalidOperationException("SetList not found");
        }
        
        var setList = setLists.First();

        var item = setList.Items.FirstOrDefault(i => i.NoteSheetId == dto.NoteSheetId);

        if (item == null)
        {
            throw new InvalidOperationException("SetList item not found");
        }

        var reorderableItems = setList.Items.OrderBy(sl => sl.Order).ToList();
        reorderableItems.RemoveAll(i => i.NoteSheetId == dto.NoteSheetId);

        if (dto.NewOrder > reorderableItems.Count)
        {
            dto.NewOrder = (uint)reorderableItems.Count;
        }

        reorderableItems.Insert((int)dto.NewOrder, item);

        for (var i = 0; i < reorderableItems.Count; i++)
        {
            reorderableItems[i].Order = (uint)i;
            unitOfWork.SetListItems.Update(reorderableItems[i]);
        }

        await unitOfWork.SaveChangesAsync();
    }
}