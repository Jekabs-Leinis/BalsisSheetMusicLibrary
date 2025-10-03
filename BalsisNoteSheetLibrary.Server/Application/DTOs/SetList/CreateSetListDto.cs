using System.ComponentModel.DataAnnotations;
using BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Server.Application.DTOs.SetList
{
    public class CreateSetListDto
    {
        [Required]
        public string Title { get; set; }
        public List<CreateSetListItemDto> Items { get; set; }

        public Domain.Entities.SetList ToEntity() => new()
        {
            Title = Title,
            Items = Items.Select(i => i.ToEntity()).ToList()
        };
    }

    public class CreateSetListItemDto
    {
        public uint NoteSheetId { get; set; }
        public uint Order { get; set; }
        
        public SetListItem ToEntity() => new()
        {
            NoteSheetId = NoteSheetId,
            Order = Order
        };
    }
}