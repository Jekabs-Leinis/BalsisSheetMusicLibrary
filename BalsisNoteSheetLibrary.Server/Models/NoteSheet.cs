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
    public bool? IsLatvian { get; set; }
    
    [JsonIgnore]
    public IEnumerable<SetListItem>? SetListItems { get; set; }
    
    // We want to serve user-friendly filenames, but we also need to ensure that the filename is unique
    // Thus on the disk we store the filename as `${Id}_${Filename}`, but we expose it as just `Filename` in the API.
    public string GetSystemFileName()
    {
        return $"{Id}_{Filename}";
    }
}