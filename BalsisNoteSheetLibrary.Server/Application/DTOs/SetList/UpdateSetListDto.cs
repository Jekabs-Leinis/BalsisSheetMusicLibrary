using System.ComponentModel.DataAnnotations;

namespace BalsisNoteSheetLibrary.Server.Application.DTOs.SetList
{
    public class UpdateSetListDto
    {
        [Required]
        public uint Id { get; set; }
        [Required]
        public string Title { get; set; }
        public List<UpdateSetListItemDto> Items { get; set; }

        public Domain.Entities.SetList ToEntity() => new()
        {
            Id = Id,
            Title = Title,
            Items = Items.Select(i => i.ToEntity()).ToList()
        };
    }

    public class UpdateSetListItemDto
    {
        public uint NoteSheetId { get; set; }
        public uint Order { get; set; }

        public Domain.Entities.SetListItem ToEntity() => new()
        {
            NoteSheetId = NoteSheetId,
            Order = Order
        };
    }
}