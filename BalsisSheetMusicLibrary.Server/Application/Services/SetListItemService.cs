using BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;
using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.Interfaces;

namespace BalsisSheetMusicLibrary.Server.Application.Services;

public class SetListItemService(IUnitOfWork unitOfWork) : ISetListItemService
{
    public async Task MoveSetListItemAsync(MoveSetListItemDto dto)
    {
        var setList = await unitOfWork.SetLists.GetByIdWithItemsAsync(dto.SetListId);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        var item = setList.Items.FirstOrDefault(i => i.SheetMusicId == dto.SheetMusicId);

        if (item == null)
        {
            throw new InvalidOperationException("SetList item not found");
        }

        var reorderableItems = setList.Items.OrderBy(sl => sl.Order).ToList();
        reorderableItems.RemoveAll(i => i.SheetMusicId == dto.SheetMusicId);

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