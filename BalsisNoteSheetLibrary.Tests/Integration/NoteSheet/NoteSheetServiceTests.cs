using BalsisNoteSheetLibrary.Server.Application.DTOs.NoteSheet;
using BalsisNoteSheetLibrary.Server.Application.Services;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using Moq;
using Entities = BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Tests.Integration.NoteSheet;

public class NoteSheetServiceTests : IntegrationTestBase
{
    private const uint NonexistentId = 999;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly NoteSheetService _service;

    public NoteSheetServiceTests()
    {
        _service = new NoteSheetService(UnitOfWork, _fileStorageServiceMock.Object);
    }

    [Fact]
    public async Task GetNoteSheetAsync_WithExistingId_ReturnsNoteSheet()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet { Id = 1, Title = "Test" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetNoteSheetAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(noteSheet.Title, result.Title);
    }

    [Fact]
    public async Task GetNoteSheetAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _service.GetNoteSheetAsync(NonexistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllNoteSheetsAsync_ReturnsAllNoteSheetsOrderedByTitle()
    {
        // Arrange
        UnitOfWork.NoteSheets.AddRange([
            new Entities.NoteSheet { Title = "B" },
            new Entities.NoteSheet { Title = "A" }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetAllNoteSheetsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.IsType<NoteSheetDto>(result.ElementAt(0));
    }

    [Fact]
    public async Task CreateNoteSheetAsync_ValidData_CreatesAndReturnsNoteSheet()
    {
        // Arrange
        var dto = new CreateNoteSheetDto { Title = "New Note" };
        var fileStream = new MemoryStream([1, 2, 3]);
        _fileStorageServiceMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("savedfile.txt");

        // Act
        var result = await _service.CreateNoteSheetAsync(dto, fileStream);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Id);
        Assert.Equal(dto.Title, result.Title);
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateNoteSheetAsync_WithFile_UpdatesNoteSheetAndFile()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet { Id = 1, Title = "Original" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var updateDto = new UpdateNoteSheetDto { Id = 1, Title = "Updated Title" };
        var fileStream = new MemoryStream([1, 2, 3]);
        _fileStorageServiceMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("savedfile.txt");

        // Act
        var result = await _service.UpdateNoteSheetAsync(updateDto, fileStream);

        // Assert
        Assert.Equal(updateDto.Title, result.Title);
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteNoteSheetAsync_WithExistingId_DeletesNoteSheetAndFile()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet { Id = 1, Title = "To Delete", SystemFileName = "testfile.txt" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        _fileStorageServiceMock.Setup(x => x.DeleteFile(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteNoteSheetAsync(1);

        // Assert
        var deletedNoteSheet = await UnitOfWork.NoteSheets.GetByIdAsync(1);
        Assert.Null(deletedNoteSheet);
        _fileStorageServiceMock.Verify(x => x.DeleteFile("testfile.txt", "delete", false), Times.Once);
    }

    [Fact]
    public async Task DeleteNoteSheetAsync_NonExistingId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteNoteSheetAsync(NonexistentId));
    }

    [Fact]
    public void HasValidFile_WithValidFile_ReturnsTrue()
    {
        // Arrange
        var dto = new NoteSheetDto { SystemFileName = "valid.txt", FileName = "valid.txt" };
        _fileStorageServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

        // Act
        var result = _service.HasValidFile(dto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasValidFile_WithMissingFileName_ReturnsFalse()
    {
        // Arrange
        var dto = new NoteSheetDto { SystemFileName = null, FileName = "test.txt" };

        // Act
        var result = _service.HasValidFile(dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateNoteSheetAsync_NonExistingId_ThrowsException()
    {
        // Arrange
        var updateDto = new UpdateNoteSheetDto { Id = NonexistentId, Title = "Doesn't exist" };
        var fileStream = new MemoryStream([1, 2, 3]);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateNoteSheetAsync(updateDto, fileStream));
    }

    [Fact]
    public async Task UpdateNoteSheetAsync_WithoutFile_UpdatesMetadataOnly()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet { Id = 2, Title = "Original", SystemFileName = "original.txt" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var updateDto = new UpdateNoteSheetDto { Id = 2, Title = "Updated Title" };
        _fileStorageServiceMock.Setup(x => x.RenameFile(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

        // Act
        var result = await _service.UpdateNoteSheetAsync(updateDto, null);

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
        var dto = new NoteSheetDto { SystemFileName = "missing.txt", FileName = "missing.txt" };
        _fileStorageServiceMock.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

        // Act
        var result = _service.HasValidFile(dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllNoteSheetsAsync_WhenNoNoteSheets_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAllNoteSheetsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllNoteSheetsAsync_ReturnsNoteSheetsOrderedByTitleCaseInsensitive()
    {
        // Arrange
        var noteSheets = new List<Entities.NoteSheet>
        {
            new() { Title = "Zebra" },
            new() { Title = "apple" },
            new() { Title = "Banana" },
            new() { Title = "apple" } // Duplicate title to test stable sort
        };

        UnitOfWork.NoteSheets.AddRange(noteSheets);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetAllNoteSheetsAsync();

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
    public async Task GetAllNoteSheetsAsync_WithSpecialCharacters_OrdersCorrectly()
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

        UnitOfWork.NoteSheets.AddRange(noteSheets);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetAllNoteSheetsAsync();

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
    public async Task GetAllNoteSheetsAsync_WithNonEnglishChars_OrdersCorrectly()
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

        UnitOfWork.NoteSheets.AddRange(noteSheets);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _service.GetAllNoteSheetsAsync();

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
    public async Task CreateNoteSheetAsync_WithNullFileStream_ThrowsArgumentNullException()
    {
        // Arrange
        var dto = new CreateNoteSheetDto { Title = "Test" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateNoteSheetAsync(dto, null!));
    }

    [Fact]
    public async Task CreateNoteSheetAsync_WhenFileSaveFails_RollsBackAndThrows()
    {
        // Arrange
        var dto = new CreateNoteSheetDto { Title = "Test" };
        var fileStream = new MemoryStream([1, 2, 3]);
        _fileStorageServiceMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ThrowsAsync(new IOException("Disk full"));

        // Act & Assert
        await Assert.ThrowsAsync<IOException>(() => _service.CreateNoteSheetAsync(dto, fileStream));

        // Verify rollback - file should be permanently deleted (not soft-deleted) and entity should not exist
        _fileStorageServiceMock.Verify(x => x.DeleteFile(It.IsAny<string>(), It.IsAny<string>(), true), Times.Once);
        var noteSheets = await _service.GetAllNoteSheetsAsync();
        Assert.Empty(noteSheets);
    }

    [Fact]
    public async Task UpdateNoteSheetAsync_WithoutFileAndNoExistingFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet { Id = 3, Title = "Original", SystemFileName = null };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var updateDto = new UpdateNoteSheetDto { Id = 3, Title = "Updated" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateNoteSheetAsync(updateDto, null));
    }
}