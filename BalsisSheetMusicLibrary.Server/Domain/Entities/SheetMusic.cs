using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;

namespace BalsisSheetMusicLibrary.Server.Domain.Entities;

public class SheetMusic
{
    public uint? Id { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Lyricist { get; set; }
    public uint? Year { get; set; }
    public string? FileName { get; set; }
    public string? SystemFileName { get; set; }
    public bool IsLatvian { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   public IEnumerable<SetListItem>? SetListItems { get; set; }

    public string GetFileName(string? extension = null)
    {
        var nameParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(Title))
        {
            nameParts.Add(Title);
        }

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

        if (nameParts.Count == 0)
        {
            throw new InvalidOperationException("At least one of Title, Author, Lyricist, or Year must be provided.");
        }

        var fileName = string.Join(", ", nameParts);
        
        fileName += extension ?? ".pdf";

        fileName = SanitizedFileName.Create(fileName);

        return fileName;
    }

    public string GetSystemFileName(string? extension = null)
    {
        if (Id is null)
        {
            throw new InvalidOperationException("Sheet id cannot be null.");
        }

        return $"{Id}_{GetFileName(extension)}";
    }
}