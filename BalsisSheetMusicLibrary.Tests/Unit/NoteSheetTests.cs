using BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Tests.Unit;

public class SheetMusicTests
{
    [Fact]
    public void GetFileName_ReturnsCorrectFileName_WithAllFields()
    {
        var sheetMusic = new SheetMusic
        {
            Title = "SongTitle",
            Author = "Composer",
            Lyricist = "LyricistName",
            Year = 2020
        };
        var fileName = sheetMusic.GetFileName();
        Assert.StartsWith("SongTitle, Composer, LyricistName, 2020", fileName);
        Assert.EndsWith(".pdf", fileName);
    }

    [Fact]
    public void GetFileName_HandlesMissingOptionalFields()
    {
        var sheetMusic = new SheetMusic { Title = "OnlyTitle" };
        var fileName = sheetMusic.GetFileName();
        Assert.StartsWith("OnlyTitle", fileName);
        Assert.EndsWith(".pdf", fileName);
    }

    [Fact]
    public void GetFileName_TruncatesLongFileName()
    {
        var longTitle = new string('A', 250);
        var sheetMusic = new SheetMusic { Title = longTitle };
        var fileName = sheetMusic.GetFileName();
        Assert.Equal(200, fileName.Length);
    }

    [Fact]
    public void GetFileName_ThrowsIfFileNameEmpty()
    {
        var sheetMusic = new SheetMusic();
        Assert.Throws<InvalidOperationException>(() => sheetMusic.GetFileName());
    }

    [Fact]
    public void GetSystemFileName_ReturnsCorrectSystemFileName_WithFileName()
    {
        var sheetMusic = new SheetMusic { Id = 5, Title = "custom" };
        var systemFileName = sheetMusic.GetSystemFileName();
        Assert.Equal("5_custom.pdf", systemFileName);
    }

    [Fact]
    public void GetSystemFileName_ReturnsCorrectSystemFileName_WithoutFileName()
    {
        var sheetMusic = new SheetMusic { Id = 7, Title = "Title" };
        var systemFileName = sheetMusic.GetSystemFileName();
        Assert.StartsWith("7_", systemFileName);
        Assert.EndsWith(".pdf", systemFileName);
    }

    [Fact]
    public void GetSystemFileName_ThrowsIfIdIsNull()
    {
        var sheetMusic = new SheetMusic();
        Assert.Throws<InvalidOperationException>(() => sheetMusic.GetSystemFileName());
    }

    [Fact]
    public void GetFileName_HandlesWhitespaceFields()
    {
        var sheetMusic = new SheetMusic { Title = "   ", Author = "   ", Lyricist = "   " };
        Assert.Throws<InvalidOperationException>(() => sheetMusic.GetFileName());
    }

    [Fact]
    public void GetFileName_SanitizesSpecialCharacters()
    {
        var sheetMusic = new SheetMusic { Title = "Test/Name:With*Invalid|Chars?" };
        var fileName = sheetMusic.GetFileName();
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
        var sheetMusic = new SheetMusic { Title = "A,Title", Author = "B,Author" };
        var fileName = sheetMusic.GetFileName();
        // Should preserve commas in the joined string
        Assert.Contains(",", fileName);
    }

    [Fact]
    public void GetFileName_HandlesUnicodeCharacters()
    {
        var sheetMusic = new SheetMusic { Title = "Тест", Author = "作者" };
        var fileName = sheetMusic.GetFileName();
        Assert.Contains("Тест", fileName);
        Assert.Contains("作者", fileName);
    }

    [Fact]
    public void GetFileName_HandlesYearZero()
    {
        var sheetMusic = new SheetMusic { Title = "Title", Year = 0 };
        var fileName = sheetMusic.GetFileName();
        Assert.Contains("0", fileName);
    }

    [Fact]
    public void GetSystemFileName_HandlesWhitespaceFileName()
    {
        var sheetMusic = new SheetMusic { Id = 10, Title = "Test", FileName = "   " };
        var systemFileName = sheetMusic.GetSystemFileName();
        Assert.StartsWith("10_", systemFileName);
        Assert.EndsWith(".pdf", systemFileName);
    }

    [Fact]
    public void GetSystemFileName_HandlesUnicodeFileName()
    {
        var sheetMusic = new SheetMusic { Id = 11, Title = "ユニコード.pdf" };
        var systemFileName = sheetMusic.GetSystemFileName();
        Assert.Contains("ユニコード", systemFileName);
    }
}