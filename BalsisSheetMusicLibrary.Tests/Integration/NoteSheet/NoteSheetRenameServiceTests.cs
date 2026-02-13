using BalsisSheetMusicLibrary.Server.Application.Services;
using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using BalsisSheetMusicLibrary.Server.Infrastructure.Hubs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Entities = BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Tests.Integration.SheetMusic;

public class SheetMusicMusicRenameServiceTests : IntegrationTestBase
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IHubContext<StatusHub>> _hubContextMock = new();
    private readonly Mock<ILogger<SheetMusicMusicRenameService>> _loggerMock = new();
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
    private readonly SheetMusicMusicRenameService _service;
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();

    public SheetMusicMusicRenameServiceTests()
    {
        // Clean up database before each test
        DbContext.SheetMusic.RemoveRange(DbContext.SheetMusic);
        DbContext.SaveChanges();

        // Set up environment mock
        _envMock.Setup(e => e.ContentRootPath).Returns("C:");

        // Set up scope factory to return a scope with our service provider
        _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(new TestScope(_serviceProviderMock.Object));
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory))).Returns(_scopeFactoryMock.Object);

        // Set up service provider to return required dependencies
        _serviceProviderMock.Setup(x => x.GetService(typeof(IUnitOfWork))).Returns(UnitOfWork);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IFileStorageService)))
            .Returns(_fileStorageServiceMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IHubContext<StatusHub>))).Returns(_hubContextMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IWebHostEnvironment))).Returns(_envMock.Object);

        _service = new SheetMusicMusicRenameService(_serviceProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task RenameAllFilenamesAsync_WithExistingFiles_UpdatesFileNamesAndDatabase()
    {
        // Arrange
        var sheetMusic = new Entities.SheetMusic
        {
            Id = 1, Title = "TestTitle", Author = "TestAuthor", FileName = "OldName.pdf", SystemFileName = "oldname.pdf"
        };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        _fileStorageServiceMock.Setup(f => f.RenameFile(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
        _fileStorageServiceMock.Setup(f => f.GetBasePath()).Returns("/");

        // Act
        await _service.RenameAllFilenamesAsync();
        // Wait for background task to complete
        await Task.Delay(500, TestContext.Current.CancellationToken);

        // Assert
        _fileStorageServiceMock.Verify(f => f.RenameFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        var updatedSheet = await UnitOfWork.SheetMusic.GetByIdAsync(sheetMusic.Id!.Value);
        Assert.Equal(updatedSheet!.FileName, updatedSheet.GetFileName());
    }

    [Fact]
    public async Task RenameAllFilenamesAsync_FileMissing_HandlesGracefully()
    {
        // Arrange
        var sheetMusic = new Entities.SheetMusic
        {
            Id = 2, Title = "MissingFile", Author = "TestAuthor", FileName = "Missing.pdf",
            SystemFileName = "missing.pdf"
        };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        _fileStorageServiceMock.Setup(f => f.RenameFile(It.IsAny<string>(), It.IsAny<string>()))
            .Throws<FileNotFoundException>();
        _fileStorageServiceMock.Setup(f => f.GetBasePath()).Returns("/");

        // Act
        await _service.RenameAllFilenamesAsync();
        await Task.Delay(500, TestContext.Current.CancellationToken);

        // Assert
        _fileStorageServiceMock.Verify(f => f.RenameFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        var updatedSheet = await UnitOfWork.SheetMusic.GetByIdAsync(sheetMusic.Id!.Value);
        // Should not update FileName/SystemFileName if file is missing
        Assert.Equal("Missing.pdf", updatedSheet!.FileName);
        Assert.Equal("missing.pdf", updatedSheet.SystemFileName);
    }

    // Helper for IServiceScope
    private class TestScope(IServiceProvider provider) : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; } = provider;

        public void Dispose()
        {
        }
    }
}