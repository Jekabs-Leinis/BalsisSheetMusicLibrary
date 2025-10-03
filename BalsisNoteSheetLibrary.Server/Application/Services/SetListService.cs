using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;

namespace BalsisNoteSheetLibrary.Server.Application.Services
{
    public class SetListService(AppDbContext context, ISetListRepository setListRepository) : ISetListService
    {
        public async Task<IEnumerable<SetListDto>> GetAllSetListsAsync()
        {
            var setLists = await setListRepository.GetAllAsync();
            
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
            var itemsToRemove = existingItems.Where(ei => updatedItems.All(ui => ui.NoteSheetId != ei.NoteSheetId)).ToList();
            if (itemsToRemove.Count > 0)
            {
                context.SetListItems.RemoveRange(itemsToRemove);
            }

            // Update existing items and add new ones
            foreach (var updatedItem in updatedItems)
            {
                var existingItem = existingItems.FirstOrDefault(ei => ei.NoteSheetId == updatedItem.NoteSheetId);
                if (existingItem != null)
                {
                    // Update properties if changed
                    existingItem.SetListId = setList.Id;
                    existingItem.NoteSheetId = updatedItem.NoteSheetId;
                    existingItem.Order = updatedItem.Order;
                }
                else
                {
                    // New item
                    updatedItem.SetListId = setList.Id;
                    setList.Items.Add(updatedItem);
                }
            }

            context.SetLists.Update(setList);
            context.SetListItems.UpdateRange(setList.Items);
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

        public async Task UpdateSetListOrderAsync(uint id, uint newOrder)
        {
            var setList = await setListRepository.GetByIdAsync(id);

            if (setList == null)
            {
                throw new InvalidOperationException("SetList not found");
            }
            
            setList.Order = newOrder;
            context.SetLists.Update(setList);
            
            await context.SaveChangesAsync();
        }
    }
}