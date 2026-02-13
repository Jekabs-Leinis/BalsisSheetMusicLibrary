using System.Security.Claims;
using BalsisSheetMusicLibrary.Server.Api.Controllers;
using BalsisSheetMusicLibrary.Server.Application.DTOs.User;
using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BalsisSheetMusicLibrary.Tests.Integration.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _controller = new UserController(_userServiceMock.Object);

        // Set up user context for authorization
        var user = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, Role.Admin)
        ], "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new() { UserName = "user1" },
            new() { UserName = "user2" },
            new() { UserName = "user3" }
        };
        _userServiceMock.Setup(x => x.GetAllUsersAsync()).ReturnsAsync(users);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsAssignableFrom<List<UserDto>>(okResult.Value);
        Assert.Equal(3, returnedUsers.Count);
    }

    [Fact]
    public async Task GetAll_WhenNoUsers_ReturnsEmptyList()
    {
        // Arrange
        _userServiceMock.Setup(x => x.GetAllUsersAsync()).ReturnsAsync([]);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUsers = Assert.IsAssignableFrom<List<UserDto>>(okResult.Value);
        Assert.Empty(returnedUsers);
    }
}
