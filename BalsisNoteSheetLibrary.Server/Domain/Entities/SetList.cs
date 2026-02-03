// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace BalsisNoteSheetLibrary.Server.Domain.Entities;

public class SetList
{
    public uint? Id { get; set; }
    public string? Title { get; set; }
    public uint? Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ArchivedAt { get; set; }

    public List<SetListItem> Items { get; set; } = [];
}