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

    [Fact]
    public void Create_WhitespaceFileName_ThrowsInvalidOperationException()
    {
        // Arrange
        const string input = "   ";
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => SanitizedFileName.Create(input));
    }

    [Fact]
    public void Create_NullFileName_ThrowsInvalidOperationException()
    {
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => SanitizedFileName.Create(null!));
    }

    [Fact]
    public void Create_FileNameWithOnlyInvalidChars_ThrowsInvalidOperationException()
    {
        // Arrange
        const string input = "***???";
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => SanitizedFileName.Create(input));
    }

    [Fact]
    public void Create_FileNameWithControlCharacters_RemovesControlChars()
    {
        // Arrange
        var input = "My\u0001Song\u001F.pdf";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("MySong.pdf", result);
    }

    [Fact]
    public void Create_FileNameWithBackslash_RemovesBackslash()
    {
        // Arrange
        const string input = "My\\Song.pdf";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("MySong.pdf", result);
    }

    [Fact]
    public void Create_FileNameWithForwardSlash_RemovesForwardSlash()
    {
        // Arrange
        const string input = "My/Song.pdf";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("MySong.pdf", result);
    }

    [Fact]
    public void Create_FileNameWithPipe_RemovesPipe()
    {
        // Arrange
        const string input = "My|Song.pdf";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("MySong.pdf", result);
    }

    [Fact]
    public void Create_LongFileName_TruncatesTo200Chars()
    {
        // Arrange
        var input = new string('a', 250) + ".pdf";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal(200, result.Length);
    }

    [Fact]
    public void Create_FileNameWithReservedWindowsNameLowercase_PrependsUnderscore()
    {
        // Arrange
        const string input = "con";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("_con", result);
    }

    [Fact]
    public void Create_FileNameWithReservedWindowsNameCOM1_PrependsUnderscore()
    {
        // Arrange
        const string input = "COM1";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("_COM1", result);
    }

    [Fact]
    public void Create_FileNameWithReservedWindowsNameLPT1_PrependsUnderscore()
    {
        // Arrange
        const string input = "LPT1";
        // Act
        var result = (string)SanitizedFileName.Create(input);
        // Assert
        Assert.Equal("_LPT1", result);
    }

    [Fact]
    public void Create_FileNameWithOnlyDotsAndSpaces_ThrowsInvalidOperationException()
    {
        // Arrange
        const string input = " . . . ";
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => SanitizedFileName.Create(input));
    }
}