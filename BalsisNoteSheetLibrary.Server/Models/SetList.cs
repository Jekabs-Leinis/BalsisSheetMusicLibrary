// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

using System.Text.Json.Serialization;

namespace BalsisNoteSheetLibrary.Server.Models;

public class SetList
{
    public uint? Id { get; set; }
    public string? Title { get; set; }
    public uint? Order { get; set; }

    [JsonIgnore] public IEnumerable<SetListItem>? Items { get; set; }
}