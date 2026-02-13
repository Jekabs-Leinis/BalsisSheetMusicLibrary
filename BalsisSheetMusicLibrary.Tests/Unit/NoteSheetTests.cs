using BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Tests.Unit;

public class NoteSheetTests
{
    [Fact]
    public void GetFileName_ReturnsCorrectFileName_WithAllFields()
    {
        var noteSheet = new NoteSheet
        {
            Title = "SongTitle",
            Author = "Composer",
            Lyricist = "LyricistName",
            Year = 2020
        };
        var fileName = noteSheet.GetFileName();
        Assert.StartsWith("SongTitle, Composer, LyricistName, 2020", fileName);
        Assert.EndsWith(".pdf", fileName);
    }

    [Fact]
    public void GetFileName_HandlesMissingOptionalFields()
    {
        var noteSheet = new NoteSheet { Title = "OnlyTitle" };
        var fileName = noteSheet.GetFileName();
        Assert.StartsWith("OnlyTitle", fileName);
        Assert.EndsWith(".pdf", fileName);
    }

    [Fact]
    public void GetFileName_TruncatesLongFileName()
    {
        var longTitle = new string('A', 250);
        var noteSheet = new NoteSheet { Title = longTitle };
        var fileName = noteSheet.GetFileName();
        // 200 chars + .pdf
        Assert.True(fileName.Length == 204);
    }

    [Fact]
    public void GetFileName_ThrowsIfFileNameEmpty()
    {
        var noteSheet = new NoteSheet();
        Assert.Throws<InvalidOperationException>(() => noteSheet.GetFileName());
    }

    [Fact]
    public void GetSystemFileName_ReturnsCorrectSystemFileName_WithFileName()
    {
        var noteSheet = new NoteSheet { Id = 5, FileName = "custom.pdf" };
        var systemFileName = noteSheet.GetSystemFileName();
        Assert.Equal("5_custom.pdf", systemFileName);
    }

    [Fact]
    public void GetSystemFileName_ReturnsCorrectSystemFileName_WithoutFileName()
    {
        var noteSheet = new NoteSheet { Id = 7, Title = "Title" };
        var systemFileName = noteSheet.GetSystemFileName();
        Assert.StartsWith("7_", systemFileName);
        Assert.EndsWith(".pdf", systemFileName);
    }

    [Fact]
    public void GetSystemFileName_ThrowsIfIdIsNull()
    {
        var noteSheet = new NoteSheet();
        Assert.Throws<InvalidOperationException>(() => noteSheet.GetSystemFileName());
    }

    [Fact]
    public void GetFileName_HandlesWhitespaceFields()
    {
        var noteSheet = new NoteSheet { Title = "   ", Author = "   ", Lyricist = "   " };
        Assert.Throws<InvalidOperationException>(() => noteSheet.GetFileName());
    }

    [Fact]
    public void GetFileName_SanitizesSpecialCharacters()
    {
        var noteSheet = new NoteSheet { Title = "Test/Name:With*Invalid|Chars?" };
        var fileName = noteSheet.GetFileName();
        // Should not contain invalid filename characters
        Assert.DoesNotContain("/", fileName);
        Assert.DoesNotContain(":", fileName);
        Assert.DoesNotContain("*", fileName);
        Assert.DoesNotContain("|", fileName);
        Assert.DoesNotContain("?", fileName);
    }

    [Fact]
    public void GetFileName_HandlesCommasInFields()
    {
        var noteSheet = new NoteSheet { Title = "A,Title", Author = "B,Author" };
        var fileName = noteSheet.GetFileName();
        // Should preserve commas in the joined string
        Assert.Contains(",", fileName);
    }

    [Fact]
    public void GetFileName_HandlesUnicodeCharacters()
    {
        var noteSheet = new NoteSheet { Title = "Тест", Author = "作者" };
        var fileName = noteSheet.GetFileName();
        Assert.Contains("Тест", fileName);
        Assert.Contains("作者", fileName);
    }

    [Fact]
    public void GetFileName_HandlesYearZero()
    {
        var noteSheet = new NoteSheet { Title = "Title", Year = 0 };
        var fileName = noteSheet.GetFileName();
        Assert.Contains("0", fileName);
    }

    [Fact]
    public void GetSystemFileName_HandlesWhitespaceFileName()
    {
        var noteSheet = new NoteSheet { Id = 10, Title = "Test", FileName = "   " };
        var systemFileName = noteSheet.GetSystemFileName();
        Assert.StartsWith("10_", systemFileName);
        Assert.EndsWith(".pdf", systemFileName);
    }

    [Fact]
    public void GetSystemFileName_HandlesUnicodeFileName()
    {
        var noteSheet = new NoteSheet { Id = 11, FileName = "ユニコード.pdf" };
        var systemFileName = noteSheet.GetSystemFileName();
        Assert.Contains("ユニコード", systemFileName);
    }
}