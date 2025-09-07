// ReSharper disable EntityFramework.ModelValidation.UnlimitedStringLength

using System.Text.Json.Serialization;

namespace BalsisNoteSheetLibrary.Server.Models;

public class NoteSheet
{
    public uint? Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Lyricist { get; set; }
    public uint? Year { get; set; }
    public string? Filename { get; set; }

    public string? SystemFileName { get; set; }

    public bool? IsLatvian { get; set; } = false;
    
    [JsonIgnore]
    public IEnumerable<SetListItem>? SetListItems { get; set; }
}