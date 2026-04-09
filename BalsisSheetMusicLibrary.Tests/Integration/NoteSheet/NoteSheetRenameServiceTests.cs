using BalsisSheetMusicLibrary.Server.Application.Services;
using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using BalsisSheetMusicLibrary.Server.Infrastructure.Hubs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Entities = BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Tests.Integration.NoteSheet;

public class SheetMusicMusicRenameServiceTests : IntegrationTestBase
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IHubContext<StatusHub>> _hubContextMock = new();
    private readonly Mock<ILogger<SheetMusicMusicRenameService>> _loggerMock = new();
    private readonly SheetMusicMusicRenameService _service;

    public SheetMusicMusicRenameServiceTests()
    {
        // Clean up database before each test
        DbContext.SheetMusic.RemoveRange(DbContext.SheetMusic);
        DbContext.SaveChanges();

        // Set up environment mock
        _envMock.Setup(e => e.ContentRootPath).Returns("C:");

        // Set up service provider with mocked services
        var services = new ServiceCollection();
        services.AddScoped<IUnitOfWork>(_ => UnitOfWork);
        services.AddScoped<IFileStorageService>(_ => _fileStorageServiceMock.Object);
        services.AddScoped<IHubContext<StatusHub>>(_ => _hubContextMock.Object);
        services.AddScoped<IWebHostEnvironment>(_ => _envMock.Object);
        var serviceProvider = services.BuildServiceProvider();

        var serviceScope = new TestScope(serviceProvider);

        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(serviceScope);

        _service = new SheetMusicMusicRenameService(serviceScopeFactory.Object, _loggerMock.Object);
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
        public void Dispose()
        {
        }

        public IServiceProvider ServiceProvider { get; } = provider;
    }
}