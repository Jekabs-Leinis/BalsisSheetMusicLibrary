using System.Security.Claims;
using BalsisSheetMusicLibrary.Server.Api.Controllers;
using BalsisSheetMusicLibrary.Server.Application.DTOs.Auth;
using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BalsisSheetMusicLibrary.Tests.Integration.Controllers;

public class AuthenticationControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly AuthenticationController _controller;

    public AuthenticationControllerTests()
    {
        _controller = new AuthenticationController(_authServiceMock.Object);

        // Set up default user context
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
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        // Arrange
        var loginDto = new LoginRequestDto { UserName = "testuser", Password = "password123" };
        var response = new LoginResponseDto { UserName = "testuser", IsAdmin = false };
        _authServiceMock.Setup(x => x.LoginAsync(loginDto)).ReturnsAsync(response);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<LoginResponseDto>(okResult.Value);
        Assert.Equal("testuser", returnedDto.UserName);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new LoginRequestDto { UserName = "testuser", Password = "wrongpassword" };
        _authServiceMock.Setup(x => x.LoginAsync(loginDto))
            .ThrowsAsync(new InvalidOperationException("Invalid username or password"));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginRequestDto { UserName = "", Password = "" };
        _controller.ModelState.AddModelError("UserName", "Required");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        // Arrange
        _authServiceMock.Setup(x => x.LogoutAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Logout();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _authServiceMock.Verify(x => x.LogoutAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCurrentUser_WithAuthenticatedUser_ReturnsOk()
    {
        // Arrange
        var currentUser = new CurrentUserDto("testuser", false);
        _authServiceMock.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync(currentUser);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDto = Assert.IsType<CurrentUserDto>(okResult.Value);
        Assert.Equal("testuser", returnedDto.UserName);
    }

    [Fact]
    public async Task GetCurrentUser_WithUnauthenticatedUser_ReturnsUnauthorized()
    {
        // Arrange
        _authServiceMock.Setup(x => x.GetCurrentUserAsync()).ReturnsAsync((CurrentUserDto?)null);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task ChangePassword_WithValidData_ReturnsOk()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentVariables.AllowManualPasswordReset, "1");
        var dto = new ChangePasswordRequestDto { UserName = "testuser", NewPassword = "newpassword123" };
        _authServiceMock.Setup(x => x.ChangePasswordAsync(dto)).Returns(Task.CompletedTask);

        try
        {
            // Act
            var result = await _controller.ChangePassword(dto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvironmentVariables.AllowManualPasswordReset, null);
        }
    }

    [Fact]
    public async Task ChangePassword_WhenDisabled_ReturnsBadRequest()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentVariables.AllowManualPasswordReset, null);
        var dto = new ChangePasswordRequestDto { UserName = "testuser", NewPassword = "newpassword123" };

        // Act
        var result = await _controller.ChangePassword(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ChangePassword_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentVariables.AllowManualPasswordReset, "1");
        var dto = new ChangePasswordRequestDto { UserName = "", NewPassword = "" };
        _controller.ModelState.AddModelError("UserName", "Required");

        try
        {
            // Act
            var result = await _controller.ChangePassword(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvironmentVariables.AllowManualPasswordReset, null);
        }
    }
}
