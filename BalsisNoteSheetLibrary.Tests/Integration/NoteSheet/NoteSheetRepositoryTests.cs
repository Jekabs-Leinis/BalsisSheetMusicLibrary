using BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;

namespace BalsisNoteSheetLibrary.Tests.Integration.NoteSheet;

public class NoteSheetRepositoryTests : IntegrationTestBase
{
    private readonly NoteSheetRepository _repository;

    public NoteSheetRepositoryTests()
    {
        _repository = new NoteSheetRepository(DbContext);

        // Ensure the database is clean before each test
        DbContext.NoteSheets.RemoveRange(DbContext.NoteSheets);
        DbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAllOrderedByTitleAsync_WhenNoNoteSheets_ReturnsEmptyList()
    {
        // Act
        var result = await _repository.GetAllOrderedByTitleAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllOrderedByTitleAsync_ReturnsNoteSheetsOrderedByTitleCaseInsensitive()
    {
        // Arrange
        var noteSheets = new List<BalsisNoteSheetLibrary.Server.Domain.Entities.NoteSheet>
        {
            new() { Title = "Zebra" },
            new() { Title = "apple" },
            new() { Title = "Banana" },
            new() { Title = "apple" } // Duplicate title to test stable sort
        };

        await DbContext.NoteSheets.AddRangeAsync(noteSheets);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllOrderedByTitleAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count);

        // Verify order (case-insensitive)
        Assert.Equal("apple", result[0].Title);
        Assert.Equal("apple", result[1].Title);
        Assert.Equal("Banana", result[2].Title);
        Assert.Equal("Zebra", result[3].Title);
    }

    [Fact]
    public async Task GetAllOrderedByTitleAsync_WithSpecialCharacters_OrdersCorrectly()
    {
        // Arrange
        var noteSheets = new List<BalsisNoteSheetLibrary.Server.Domain.Entities.NoteSheet>
        {
            new() { Title = "1. First" },
            new() { Title = "Third" },
            new() { Title = "#Special" },
            new() { Title = "Second" },
            new() { Title = "!First" }
        };

        await DbContext.NoteSheets.AddRangeAsync(noteSheets);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllOrderedByTitleAsync();

        // Assert
        var titles = result.Select(ns => ns.Title).ToList();

        // The order should be based on the SQLite collation rules
        // Special characters typically come before alphanumeric characters
        Assert.Equal("!First", titles[0]);
        Assert.Equal("#Special", titles[1]);
        Assert.Equal("1. First", titles[2]);
        Assert.Equal("Second", titles[3]);
        Assert.Equal("Third", titles[4]);
    }

    [Fact]
    public async Task GetAllOrderedByTitleAsync_WithNonEnglishChars_OrdersCorrectly()
    {
        // Arrange
        var noteSheets = new List<BalsisNoteSheetLibrary.Server.Domain.Entities.NoteSheet>
        {
            new() { Title = "A1First" },
            new() { Title = "Ā4Fourth" },
            new() { Title = "B5Fifth" },
            new() { Title = "ā3Third" },
            new() { Title = "a2Second" },
            new() { Title = "Д!Last" }
        };

        await DbContext.NoteSheets.AddRangeAsync(noteSheets);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllOrderedByTitleAsync();

        // Assert
        var titles = result.Select(ns => ns.Title).ToList();

        // The order should be based on the SQLite collation rules
        // Diacritics are considered equivalent to their base characters in a case-insensitive manner
        // and non-Latin characters are sorted after Latin characters
        Assert.Equal("A1First", titles[0]);
        Assert.Equal("a2Second", titles[1]);
        Assert.Equal("ā3Third", titles[2]);
        Assert.Equal("Ā4Fourth", titles[3]);
        Assert.Equal("B5Fifth", titles[4]);
        Assert.Equal("Д!Last", titles[5]);
    }
}