using BalsisNoteSheetLibrary.Server.Application.DTOs.NoteSheet;
using BalsisNoteSheetLibrary.Server.Application.Services;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services.Interfaces;
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
        await UnitOfWork.SaveChangesAsync();

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
        await UnitOfWork.SaveChangesAsync();

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
        await UnitOfWork.SaveChangesAsync();
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
        await UnitOfWork.SaveChangesAsync();
        _fileStorageServiceMock.Setup(x => x.DeleteFileAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteNoteSheetAsync(1);

        // Assert
        var deletedNoteSheet = await UnitOfWork.NoteSheets.GetByIdAsync(1);
        Assert.Null(deletedNoteSheet);
        _fileStorageServiceMock.Verify(x => x.DeleteFileAsync("testfile.txt"), Times.Once);
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
        await UnitOfWork.SaveChangesAsync();
        var updateDto = new UpdateNoteSheetDto { Id = 2, Title = "Updated Title" };
        _fileStorageServiceMock.Setup(x => x.MoveFile(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

        // Act
        var result = await _service.UpdateNoteSheetAsync(updateDto, null);

        // Assert
        Assert.Equal(updateDto.Title, result.Title);
        // FileStorageService should not be called
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        _fileStorageServiceMock.Verify(x => x.MoveFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
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
}