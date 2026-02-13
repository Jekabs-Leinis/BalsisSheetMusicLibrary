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

public class SetListItemControllerTests : IntegrationTestBase
{
    private readonly Mock<ILogger<SetListItemController>> _loggerMock = new();
    private readonly SetListItemController _controller;

    public SetListItemControllerTests()
    {
        var setListItemService = new SetListItemService(UnitOfWork);
        _controller = new SetListItemController(setListItemService, _loggerMock.Object);

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
    public async Task Move_WithValidData_ReturnsOk()
    {
        // Arrange
        var noteSheet1 = new Entities.NoteSheet { Id = 1, Title = "Note 1" };
        var noteSheet2 = new Entities.NoteSheet { Id = 2, Title = "Note 2" };
        var setList = new Entities.SetList { Id = 1, Title = "Set" };
        
        UnitOfWork.NoteSheets.AddRange([noteSheet1, noteSheet2]);
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var item1 = new Entities.SetListItem { SetListId = 1, NoteSheetId = 1, Order = 1 };
        var item2 = new Entities.SetListItem { SetListId = 1, NoteSheetId = 2, Order = 2 };
        UnitOfWork.SetListItems.AddRange([item1, item2]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new MoveSetListItemDto { SetListId = 1, NoteSheetId = 1, NewOrder = 2 };

        // Act
        var result = await _controller.Move(dto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Move_WithInvalidSetListId_ReturnsBadRequest()
    {
        // Arrange
        var dto = new MoveSetListItemDto { SetListId = 999, NoteSheetId = 1, NewOrder = 1 };

        // Act
        var result = await _controller.Move(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Move_WithInvalidNoteSheetId_ReturnsBadRequest()
    {
        // Arrange
        var setList = new Entities.SetList { Id = 1, Title = "Set" };
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var dto = new MoveSetListItemDto { SetListId = 1, NoteSheetId = 999, NewOrder = 1 };

        // Act
        var result = await _controller.Move(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Move_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var dto = new MoveSetListItemDto { SetListId = 1, NoteSheetId = 1, NewOrder = 1 };
        _controller.ModelState.AddModelError("SetListId", "Required");

        // Act
        var result = await _controller.Move(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }
}
