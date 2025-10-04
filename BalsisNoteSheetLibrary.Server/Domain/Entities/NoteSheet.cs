using System.Text.Json.Serialization;
using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;

namespace BalsisNoteSheetLibrary.Server.Domain.Entities;

public class NoteSheet
{
    public uint? Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Lyricist { get; set; }
    public uint? Year { get; set; }
    public string? FileName { get; set; }
    public string? SystemFileName { get; set; }
    public bool IsLatvian { get; set; }
    
    [JsonIgnore]
    public IEnumerable<SetListItem>? SetListItems { get; set; }
    
    public string GetFileName()
    {
        var nameParts = new List<string> { Title ?? "" };

        if (!string.IsNullOrWhiteSpace(Author))
        {
            nameParts.Add(Author);
        }

        if (!string.IsNullOrWhiteSpace(Lyricist))
        {
            nameParts.Add(Lyricist);
        }

        if (Year is not null)
        {
            nameParts.Add(Year.ToString() ?? string.Empty);
        }

        var fileName = string.Join(", ", nameParts);

        fileName = SanitizedFileName.Create(fileName);

        // Windows paths have a maximum length of 260 characters,
        // but filenames should be shorter to account for folder paths
        if (fileName.Length > 200)
        {
            fileName = fileName[..200];
        }

        if (fileName.Length == 0)
        {
            throw new InvalidOperationException("File name cannot be empty.");
        }

        return fileName + ".pdf";
    }

    public string GetSystemFileName()
    {
        if (Id is null)
        {
            throw new InvalidOperationException("Sheet id cannot be null.");
        }

        // If filename has already been set, trust it
        return string.IsNullOrEmpty(FileName)
            ? $"{Id}_{GetFileName()}"
            : $"{Id}_{FileName}";
    }
}