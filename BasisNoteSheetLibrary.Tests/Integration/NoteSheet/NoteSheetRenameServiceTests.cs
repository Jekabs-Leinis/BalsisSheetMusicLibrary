using BalsisNoteSheetLibrary.Server.Application.Services;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Hubs;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Entities = BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BasisNoteSheetLibrary.Tests.Integration.NoteSheet;

public class NoteSheetRenameServiceTests : IntegrationTestBase
{
    private readonly Mock<IWebHostEnvironment> _envMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IHubContext<StatusHub>> _hubContextMock = new();
    private readonly Mock<ILogger<NoteSheetRenameService>> _loggerMock = new();
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
    private readonly NoteSheetRenameService _service;
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();

    public NoteSheetRenameServiceTests()
    {
        // Clean up database before each test
        DbContext.NoteSheets.RemoveRange(DbContext.NoteSheets);
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

        _service = new NoteSheetRenameService(_serviceProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task RenameAllFilenamesAsync_WithExistingFiles_UpdatesFileNamesAndDatabase()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet
        {
            Id = 1, Title = "TestTitle", Author = "TestAuthor", FileName = "OldName.pdf", SystemFileName = "oldname.pdf"
        };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync();

        _fileStorageServiceMock.Setup(f => f.MoveFile(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

        // Act
        await _service.RenameAllFilenamesAsync();
        // Wait for background task to complete
        await Task.Delay(500);

        // Assert
        _fileStorageServiceMock.Verify(f => f.MoveFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        var updatedSheet = await UnitOfWork.NoteSheets.GetByIdAsync(noteSheet.Id!.Value);
        Assert.Equal(updatedSheet!.FileName, updatedSheet.GetFileName());
    }

    [Fact]
    public async Task RenameAllFilenamesAsync_FileMissing_HandlesGracefully()
    {
        // Arrange
        var noteSheet = new Entities.NoteSheet
        {
            Id = 2, Title = "MissingFile", Author = "TestAuthor", FileName = "Missing.pdf",
            SystemFileName = "missing.pdf"
        };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync();

        _fileStorageServiceMock.Setup(f => f.MoveFile(It.IsAny<string>(), It.IsAny<string>()))
            .Throws<FileNotFoundException>();

        // Act
        await _service.RenameAllFilenamesAsync();
        await Task.Delay(500);

        // Assert
        _fileStorageServiceMock.Verify(f => f.MoveFile(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        var updatedSheet = await UnitOfWork.NoteSheets.GetByIdAsync(noteSheet.Id!.Value);
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