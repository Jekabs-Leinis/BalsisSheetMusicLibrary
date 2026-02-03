using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;

namespace BalsisNoteSheetLibrary.Tests.Unit;

public class SanitizeFileNameTests
{
    [Fact]
    public void Create_ValidFileName_ReturnsSameName()
    {
        // Arrange
        const string input = "MySong.pdf";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("MySong.pdf", result);
    }

    [Fact]
    public void Create_FileNameWithInvalidChars_RemovesInvalidChars()
    {
        // Arrange
        const string input = "My:Song*?<>.pdf";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("MySong.pdf", result);
    }

    [Fact]
    public void Create_FileNameWithLeadingAndTrailingDotsAndSpaces_TrimsThem()
    {
        // Arrange
        const string input = "  .MySong. .pdf.  ";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("MySong. .pdf", result);
    }

    [Fact]
    public void Create_FileNameWithDirectoryTraversal_RemovesPath()
    {
        // Arrange
        const string input = "../secret.txt";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("secret.txt", result);
    }

    [Fact]
    public void Create_FileNameWithReservedWindowsName_PrependsUnderscore()
    {
        // Arrange
        const string input = "CON";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("_CON", result);
    }

    [Fact]
    public void Create_FileNameWithHash_RemovesHash()
    {
        // Arrange
        const string input = "My#Song.pdf";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("MySong.pdf", result);
    }

    [Fact]
    public void Create_EmptyFileName_ThrowsInvalidOperationException()
    {
        // Arrange
        const string input = "";
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => SanitizedFileName.Create(input));
    }
}