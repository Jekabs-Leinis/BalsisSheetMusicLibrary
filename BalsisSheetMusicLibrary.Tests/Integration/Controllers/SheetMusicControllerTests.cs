using System.Security.Claims;
using BalsisSheetMusicLibrary.Server.Api.Controllers;
using BalsisSheetMusicLibrary.Server.Application.DTOs.SheetMusic;
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

public class SheetMusicControllerTests : IntegrationTestBase
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<ILogger<SheetMusicController>> _loggerMock = new();
    private readonly Mock<ISheetMusicRenameService> _renameServiceMock = new();
    private readonly SheetMusicController _musicController;
    private readonly ISheetMusicService _sheetMusicService;

    public SheetMusicControllerTests()
    {
        _sheetMusicService = new SheetMusicMusicService(UnitOfWork, _fileStorageServiceMock.Object);
        _musicController = new SheetMusicController(
            _sheetMusicService,
            _renameServiceMock.Object,
            _loggerMock.Object
        );

        // Set up user context for authorization
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, Role.Admin)
        ], "mock"));

        _musicController.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithSheetMusic()
    {
        // Arrange
        UnitOfWork.SheetMusic.AddRange([
            new Entities.SheetMusic { Title = "A" },
            new Entities.SheetMusic { Title = "B" }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _musicController.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var SheetMusic = Assert.IsAssignableFrom<List<SheetMusicDto>>(okResult.Value);
        Assert.Equal(2, SheetMusic.Count);
    }

    [Fact]
    public async Task Get_WithExistingId_ReturnsOkWithSheetMusic()
    {
        // Arrange
        var sheetMusic = new Entities.SheetMusic { Id = 1, Title = "Test" };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _musicController.Get(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<SheetMusicDto>(okResult.Value);
        Assert.Equal("Test", dto.Title);
    }

    [Fact]
    public async Task Get_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _musicController.Get(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Add_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreateSheetMusicDto { Title = "New Sheet" };
        var fileMock = new Mock<IFormFile>();
        var content = "PDF content"u8.ToArray();
        var ms = new MemoryStream(content);
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        _fileStorageServiceMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("saved.pdf");

        // Act
        var result = await _musicController.Add(dto, fileMock.Object);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(SheetMusicController.Get), createdResult.ActionName);
        var returnedDto = Assert.IsType<SheetMusicDto>(createdResult.Value);
        Assert.Equal("New Sheet", returnedDto.Title);
    }

    [Fact]
    public async Task Add_WithEmptyFile_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateSheetMusicDto { Title = "Test" };
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        // Act
        var result = await _musicController.Add(dto, fileMock.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Add_WithInvalidFileType_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateSheetMusicDto { Title = "Test" };
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");

        // Act
        var result = await _musicController.Add(dto, fileMock.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOk()
    {
        // Arrange
        var sheetMusic = new Entities.SheetMusic { Id = 1, Title = "Original", SystemFileName = "original.pdf" };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new UpdateSheetMusicDto { Id = 1, Title = "Updated" };
        _fileStorageServiceMock.Setup(x => x.RenameFile(It.IsAny<string>(), It.IsAny<string>()));

        // Act
        var result = await _musicController.Update(dto, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<SheetMusicDto>(okResult.Value);
        Assert.Equal("Updated", returnedDto.Title);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var dto = new UpdateSheetMusicDto { Id = 999, Title = "Test" };

        // Act
        var result = await _musicController.Update(dto, null);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WithExistingId_ReturnsOk()
    {
        // Arrange
        var sheetMusic = new Entities.SheetMusic { Id = 1, Title = "To Delete", SystemFileName = "test.pdf" };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        _fileStorageServiceMock.Setup(x => x.DeleteFile(It.IsAny<string>(), It.IsAny<string>(), false)).Returns(Task.CompletedTask);

        // Act
        var result = await _musicController.Delete(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Delete_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _musicController.Delete(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task RenameAllFilenames_ReturnsOk()
    {
        // Arrange
        _renameServiceMock.Setup(x => x.RenameAllFilenamesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _musicController.RenameAllFilenames();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _renameServiceMock.Verify(x => x.RenameAllFilenamesAsync(), Times.Once);
    }
}
