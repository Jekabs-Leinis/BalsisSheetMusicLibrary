using BalsisSheetMusicLibrary.Server.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace BalsisSheetMusicLibrary.Tests.Integration.Controllers;

public class AntiforgeryControllerTests
{
    private readonly AntiforgeryController _controller;

    public AntiforgeryControllerTests()
    {
        _controller = new AntiforgeryController();
    }

    [Fact]
    public void Token_ReturnsOk()
    {
        // Act
        var result = _controller.Token();

        // Assert
        Assert.IsType<OkResult>(result);
    }
}
