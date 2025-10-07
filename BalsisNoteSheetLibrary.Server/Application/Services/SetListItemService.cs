using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;

namespace BalsisNoteSheetLibrary.Server.Application.Services;

public class SetListItemService(AppDbContext context, ISetListRepository setListRepository) : ISetListItemService
{
    public async Task MoveSetListItemAsync(MoveSetListItemDto dto)
    {
        var setList = await setListRepository.GetByIdAsync(dto.SetListId);

        if (setList == null)
        {
            throw new InvalidOperationException("SetList not found");
        }

        var item = setList.Items.FirstOrDefault(i => i.NoteSheetId == dto.NoteSheetId);

        if (item == null)
        {
            throw new InvalidOperationException("SetList item not found");
        }

        var reorderableItems = setList.Items.OrderBy(sl => sl.Order).ToList();
        reorderableItems.RemoveAll(i => i.NoteSheetId == dto.NoteSheetId);
        reorderableItems.Insert((int)dto.NewOrder, item);

        for (var i = 0; i < reorderableItems.Count; i++)
        {
            reorderableItems[i].Order = (uint)i;
            context.SetListItems.Update(reorderableItems[i]);
        }

        await context.SaveChangesAsync();
    }
}