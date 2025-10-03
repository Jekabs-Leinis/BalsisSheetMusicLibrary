namespace BalsisNoteSheetLibrary.Server.Application.DTOs.SetList
{
    public class SetListDto
    {
        public uint? Id { get; set; }
        public string? Title { get; set; }
        public uint? Order { get; set; }
        public List<SetListItemDto> Items { get; set; }
        public DateTime? ArchivedAt { get; set; }
        
        public static SetListDto FromEntity(Domain.Entities.SetList entity) => new()
        {
            Id = entity.Id,
            Title = entity.Title,
            Order = entity.Order,
            Items = entity.Items.Select(SetListItemDto.FromEntity).ToList(),
            ArchivedAt = entity.ArchivedAt
        };
    }
}