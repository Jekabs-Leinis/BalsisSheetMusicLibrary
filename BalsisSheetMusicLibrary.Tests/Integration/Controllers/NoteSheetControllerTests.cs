using System.Security.Claims;
using BalsisSheetMusicLibrary.Server.Api.Controllers;
using BalsisSheetMusicLibrary.Server.Application.DTOs.NoteSheet;
using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Application.Services;
using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Entities = BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Tests.Integration.Controllers;

public class NoteSheetControllerTests : IntegrationTestBase
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<ILogger<NoteSheetController>> _loggerMock = new();
    private readonly Mock<INoteSheetRenameService> _renameServiceMock = new();
    private readonly NoteSheetController _controller;
    private readonly INoteSheetService _noteSheetService;

    public NoteSheetControllerTests()
    {
        _noteSheetService = new NoteSheetService(UnitOfWork, _fileStorageServiceMock.Object);
        _controller = new NoteSheetController(
            _noteSheetService,
            _renameServiceMock.Object,
            _loggerMock.Object
        );

        // Set up user context for authorization
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, Role.Admin)
        ], "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithNoteSheets()
    {
        // Arrange
        UnitOfWork.NoteSheets.AddRange([
            new Entities.NoteSheet { Title = "A" },
            new Entities.NoteSheet { Title = "B" }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var noteSheets = Assert.IsAssignableFrom<List<NoteSheetDto>>(okResult.Value);
        Assert.Equal(2, noteSheets.Count);
    }

    [Fact]
    public async Task Get_WithExistingId_ReturnsOkWithNoteSheet()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet { Id = 1, Title = "Test" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _controller.Get(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<NoteSheetDto>(okResult.Value);
        Assert.Equal("Test", dto.Title);
    }

    [Fact]
    public async Task Get_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Get(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Add_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreateNoteSheetDto { Title = "New Note" };
        var fileMock = new Mock<IFormFile>();
        var content = "PDF content"u8.ToArray();
        var ms = new MemoryStream(content);
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        _fileStorageServiceMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("saved.pdf");

        // Act
        var result = await _controller.Add(dto, fileMock.Object);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(NoteSheetController.Get), createdResult.ActionName);
        var returnedDto = Assert.IsType<NoteSheetDto>(createdResult.Value);
        Assert.Equal("New Note", returnedDto.Title);
    }

    [Fact]
    public async Task Add_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateNoteSheetDto { Title = "Test" };
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _controller.Add(dto, fileMock.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Add_WithInvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateNoteSheetDto { Title = "Test" };
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        // Act
        var result = await _controller.Add(dto, fileMock.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOk()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet { Id = 1, Title = "Original", SystemFileName = "original.pdf" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new UpdateNoteSheetDto { Id = 1, Title = "Updated" };
        _fileStorageServiceMock.Setup(x => x.RenameFile(It.IsAny<string>(), It.IsAny<string>()));

        // Act
        var result = await _controller.Update(dto, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<NoteSheetDto>(okResult.Value);
        Assert.Equal("Updated", returnedDto.Title);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var dto = new UpdateNoteSheetDto { Id = 999, Title = "Test" };

        // Act
        var result = await _controller.Update(dto, null);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WithExistingId_ReturnsOk()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet { Id = 1, Title = "To Delete", SystemFileName = "test.pdf" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        _fileStorageServiceMock.Setup(x => x.DeleteFile(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Delete(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RenameAllFilenames_ReturnsOk()
    {
        // Arrange
        _renameServiceMock.Setup(x => x.RenameAllFilenamesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RenameAllFilenames();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _renameServiceMock.Verify(x => x.RenameAllFilenamesAsync(), Times.Once);
    }
}
