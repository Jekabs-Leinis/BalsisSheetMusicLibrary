using BalsisSheetMusicLibrary.Server.Application.DTOs.SheetMusic;
using BalsisSheetMusicLibrary.Server.Application.Services;
using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using Moq;
using Entities = BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Tests.Integration.SheetMusic;

public class SheetMusicMusicServiceTests : IntegrationTestBase
{
    private const uint NonexistentId = 999;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly SheetMusicMusicService _sheetMusicService;

    public SheetMusicMusicServiceTests()
    {
        _sheetMusicService = new SheetMusicMusicService(UnitOfWork, _fileStorageServiceMock.Object);
    }

    [Fact]
    public async Task GetSheetMusicAsync_WithExistingId_ReturnsSheetMusic()
    {
        // Arrange
        var sheetMusic = new Entities.SheetMusic { Id = 1, Title = "Test" };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sheetMusicService.GetSheetMusicAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sheetMusic.Title, result.Title);
    }

    [Fact]
    public async Task GetSheetMusicAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _sheetMusicService.GetSheetMusicAsync(NonexistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllSheetMusicAsync_ReturnsAllSheetMusicOrderedByTitle()
    {
        // Arrange
        UnitOfWork.SheetMusic.AddRange([
            new Entities.SheetMusic { Title = "B" },
            new Entities.SheetMusic { Title = "A" }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sheetMusicService.GetAllSheetMusicAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.IsType<SheetMusicDto>(result.ElementAt(0));
    }

    [Fact]
    public async Task CreateSheetMusicAsync_ValidData_CreatesAndReturnsSheetMusic()
    {
        // Arrange
        var dto = new CreateSheetMusicDto { Title = "New Sheet" };
        var fileStream = new MemoryStream([1, 2, 3]);
        _fileStorageServiceMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("savedfile.txt");

        // Act
        var result = await _sheetMusicService.CreateSheetMusicAsync(dto, fileStream);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal(dto.Title, result.Title);
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSheetMusicAsync_WithFile_UpdatesSheetMusicAndFile()
    {
        // Arrange
        var sheetMusic = new Entities.SheetMusic { Id = 1, Title = "Original" };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var updateDto = new UpdateSheetMusicDto { Id = 1, Title = "Updated Title" };
        var fileStream = new MemoryStream([1, 2, 3]);
        _fileStorageServiceMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("savedfile.txt");

        // Act
        var result = await _sheetMusicService.UpdateSheetMusicAsync(updateDto, fileStream);

        // Assert
        Assert.Equal(updateDto.Title, result.Title);
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSheetMusicAsync_WithExistingId_DeletesSheetMusicAndFile()
    {
        // Arrange
        var sheetMusic = new Entities.SheetMusic { Id = 1, Title = "To Delete", SystemFileName = "testfile.txt" };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        _fileStorageServiceMock.Setup(x => x.DeleteFile(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(Task.CompletedTask);

        // Act
        await _sheetMusicService.DeleteSheetMusicAsync(1);

        // Assert
        var deletedSheetMusic = await UnitOfWork.SheetMusic.GetByIdAsync(1);
        Assert.Null(deletedSheetMusic);
        _fileStorageServiceMock.Verify(x => x.DeleteFile("testfile.txt", "delete", false), Times.Once);
    }

    [Fact]
    public async Task DeleteSheetMusicAsync_NonExistingId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sheetMusicService.DeleteSheetMusicAsync(NonexistentId));
    }

    [Fact]
    public void HasValidFile_WithValidFile_ReturnsTrue()
    {
        // Arrange
        var dto = new SheetMusicDto { SystemFileName = "valid.txt", FileName = "valid.txt" };
        _fileStorageServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

        // Act
        var result = _sheetMusicService.HasValidFile(dto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasValidFile_WithMissingFileName_ReturnsFalse()
    {
        // Arrange
        var dto = new SheetMusicDto { SystemFileName = null, FileName = "test.txt" };

        // Act
        var result = _sheetMusicService.HasValidFile(dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateSheetMusicAsync_NonExistingId_ThrowsException()
    {
        // Arrange
        var updateDto = new UpdateSheetMusicDto { Id = NonexistentId, Title = "Doesn't exist" };
        var fileStream = new MemoryStream([1, 2, 3]);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sheetMusicService.UpdateSheetMusicAsync(updateDto, fileStream));
    }

    [Fact]
    public async Task UpdateSheetMusicAsync_WithoutFile_UpdatesMetadataOnly()
    {
        // Arrange
        var sheetMusic = new Entities.SheetMusic { Id = 2, Title = "Original", SystemFileName = "original.txt" };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var updateDto = new UpdateSheetMusicDto { Id = 2, Title = "Updated Title" };
        _fileStorageServiceMock.Setup(x => x.RenameFile(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

        // Act
        var result = await _sheetMusicService.UpdateSheetMusicAsync(updateDto, null);

        // Assert
        Assert.Equal(updateDto.Title, result.Title);
        // FileStorageService should not be called
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        _fileStorageServiceMock.Verify(x => x.RenameFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void HasValidFile_WithSystemFileNameButFileMissing_ReturnsFalse()
    {
        // Arrange
        var dto = new SheetMusicDto { SystemFileName = "missing.txt", FileName = "missing.txt" };
        _fileStorageServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var result = _sheetMusicService.HasValidFile(dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllSheetMusicAsync_WhenNoSheetMusic_ReturnsEmptyList()
    {
        // Act
        var result = await _sheetMusicService.GetAllSheetMusicAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllSheetMusicAsync_ReturnsSheetMusicOrderedByTitleCaseInsensitive()
    {
        // Arrange
        var SheetMusic = new List<Entities.SheetMusic>
        {
            new() { Title = "Zebra" },
            new() { Title = "apple" },
            new() { Title = "Banana" },
            new() { Title = "apple" } // Duplicate title to test stable sort
        };

        UnitOfWork.SheetMusic.AddRange(SheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sheetMusicService.GetAllSheetMusicAsync();

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
    public async Task GetAllSheetMusicAsync_WithSpecialCharacters_OrdersCorrectly()
    {
        // Arrange
        var SheetMusic = new List<BalsisSheetMusicLibrary.Server.Domain.Entities.SheetMusic>
        {
            new() { Title = "1. First" },
            new() { Title = "Third" },
            new() { Title = "#Special" },
            new() { Title = "Second" },
            new() { Title = "!First" }
        };

        UnitOfWork.SheetMusic.AddRange(SheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sheetMusicService.GetAllSheetMusicAsync();

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
    public async Task GetAllSheetMusicAsync_WithNonEnglishChars_OrdersCorrectly()
    {
        // Arrange
        var SheetMusic = new List<BalsisSheetMusicLibrary.Server.Domain.Entities.SheetMusic>
        {
            new() { Title = "A1First" },
            new() { Title = "Ā4Fourth" },
            new() { Title = "B5Fifth" },
            new() { Title = "ā3Third" },
            new() { Title = "a2Second" },
            new() { Title = "Д!Last" }
        };

        UnitOfWork.SheetMusic.AddRange(SheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _sheetMusicService.GetAllSheetMusicAsync();

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

    [Fact]
    public async Task CreateSheetMusicAsync_WithNullFileStream_ThrowsArgumentNullException()
    {
        // Arrange
        var dto = new CreateSheetMusicDto { Title = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _sheetMusicService.CreateSheetMusicAsync(dto, null!));
    }

    [Fact]
    public async Task CreateSheetMusicAsync_WhenFileSaveFails_RollsBackAndThrows()
    {
        // Arrange
        var dto = new CreateSheetMusicDto { Title = "Test" };
        var fileStream = new MemoryStream([1, 2, 3]);
        _fileStorageServiceMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ThrowsAsync(new IOException("Disk full"));

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(() => _sheetMusicService.CreateSheetMusicAsync(dto, fileStream));

        // Verify rollback - file should be permanently deleted (not soft-deleted) and entity should not exist
        _fileStorageServiceMock.Verify(x => x.DeleteFile(It.IsAny<string>(), It.IsAny<string>(), true), Times.Once);
        var SheetMusic = await _sheetMusicService.GetAllSheetMusicAsync();
        Assert.Empty(SheetMusic);
    }

    [Fact]
    public async Task UpdateSheetMusicAsync_WithoutFileAndNoExistingFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var sheetMusic = new Entities.SheetMusic { Id = 3, Title = "Original", SystemFileName = null };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var updateDto = new UpdateSheetMusicDto { Id = 3, Title = "Updated" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sheetMusicService.UpdateSheetMusicAsync(updateDto, null));
    }
}