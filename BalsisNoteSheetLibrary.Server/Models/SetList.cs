// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

namespace BalsisNoteSheetLibrary.Server.Models;

public class SetList
{
    public uint? Id { get; set; }
    public string? Title { get; set; }
    public uint? Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ArchivedAt { get; set; }

    public IEnumerable<SetListItem> Items { get; set; } = new List<SetListItem>();
}