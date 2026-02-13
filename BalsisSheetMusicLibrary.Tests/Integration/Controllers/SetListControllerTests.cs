using System.Security.Claims;
using BalsisSheetMusicLibrary.Server.Api.Controllers;
using BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;
using BalsisSheetMusicLibrary.Server.Application.Services;
using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Entities = BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Tests.Integration.Controllers;

public class SetListControllerTests : IntegrationTestBase
{
    private readonly Mock<ILogger<SetListController>> _loggerMock = new();
    private readonly SetListController _controller;

    public SetListControllerTests()
    {
        var setListService = new SetListService(UnitOfWork);
        _controller = new SetListController(setListService, _loggerMock.Object);

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
    public async Task GetAll_ReturnsOkWithSetLists()
    {
        // Arrange
        UnitOfWork.SetLists.AddRange([
            new Entities.SetList { Title = "Set 1" },
            new Entities.SetList { Title = "Set 2" }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var setLists = Assert.IsAssignableFrom<IEnumerable<SetListDto>>(okResult.Value);
        Assert.Equal(2, setLists.Count());
    }

    [Fact]
    public async Task GetAll_WithNoteSheets_ReturnsSetListsWithItems()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet { Id = 1, Title = "Note" };
        var setList = new Entities.SetList { Id = 1, Title = "Set" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var item = new Entities.SetListItem { SetListId = 1, NoteSheetId = 1, Order = 1 };
        UnitOfWork.SetListItems.Add(item);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _controller.GetAll(withNoteSheets: true);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var setLists = Assert.IsAssignableFrom<IEnumerable<SetListDto>>(okResult.Value);
        var setListDto = setLists.First();
        Assert.NotEmpty(setListDto.Items);
    }

    [Fact]
    public async Task GetAllArchived_ReturnsArchivedSetLists()
    {
        // Arrange
        var archivedSetList = new Entities.SetList
        {
            Title = "Archived",
            ArchivedAt = DateTime.UtcNow
        };
        var activeSetList = new Entities.SetList { Title = "Active" };
        UnitOfWork.SetLists.AddRange([archivedSetList, activeSetList]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _controller.GetAllArchived();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var setLists = Assert.IsAssignableFrom<IEnumerable<SetListDto>>(okResult.Value);
        Assert.Single(setLists);
        Assert.Equal("Archived", setLists.First().Title);
    }

    [Fact]
    public async Task Get_WithExistingId_ReturnsOkWithSetList()
    {
        // Arrange
        var setList = new Entities.SetList { Id = 1, Title = "Test" };
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _controller.Get(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<SetListDto>(okResult.Value);
        Assert.Equal("Test", dto.Title);
    }

    [Fact]
    public async Task Get_WithNonExistingId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Get(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Add_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var dto = new CreateSetListDto { Title = "New Set" };

        // Act
        var result = await _controller.Add(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(SetListController.Get), createdResult.ActionName);
        var returnedDto = Assert.IsType<SetListDto>(createdResult.Value);
        Assert.Equal("New Set", returnedDto.Title);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsOk()
    {
        // Arrange
        var setList = new Entities.SetList { Id = 1, Title = "Original" };
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new UpdateSetListDto { Id = 1, Title = "Updated", Items = [] };

        // Act
        var result = await _controller.Update(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<SetListDto>(okResult.Value);
        Assert.Equal("Updated", returnedDto.Title);
    }

    [Fact]
    public async Task Update_WithNonExistingId_ReturnsNotFound()
    {
        // Arrange
        var dto = new UpdateSetListDto { Id = 999, Title = "Test", Items = [] };

        // Act
        var result = await _controller.Update(dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Delete_WithExistingId_ReturnsOk()
    {
        // Arrange
        var setList = new Entities.SetList { Id = 1, Title = "To Delete" };
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Move_WithValidData_ReturnsOk()
    {
        // Arrange
        UnitOfWork.SetLists.AddRange([
            new Entities.SetList { Id = 1, Title = "First", Order = 0 },
            new Entities.SetList { Id = 2, Title = "Second", Order = 1 }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new MoveSetListDto { Id = 1, NewOrder = 1 };

        // Act
        var result = await _controller.Move(dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
    
    [Fact]
    public async Task Move_WithOutOfBoundsOrder_ReturnsBadRequest()
    {
        // Arrange
        UnitOfWork.SetLists.AddRange([
            new Entities.SetList { Id = 1, Title = "First", Order = 0 },
            new Entities.SetList { Id = 2, Title = "Second", Order = 1 }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new MoveSetListDto { Id = 1, NewOrder = 999 };

        // Act
        var result = await _controller.Move(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Archive_WithExistingId_ReturnsOk()
    {
        // Arrange
        var setList = new Entities.SetList { Id = 1, Title = "To Archive" };
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _controller.Archive(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Restore_WithArchivedId_ReturnsOk()
    {
        // Arrange
        var setList = new Entities.SetList
        {
            Id = 1,
            Title = "Archived",
            ArchivedAt = DateTime.UtcNow
        };
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _controller.Restore(1);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
