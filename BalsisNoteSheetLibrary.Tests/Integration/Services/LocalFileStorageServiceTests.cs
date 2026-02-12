using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace BalsisNoteSheetLibrary.Tests.Integration.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly string _testBasePath;
    private readonly Mock<IHostEnvironment> _hostEnvironmentMock;
    private readonly Mock<ILogger<LocalFileStorageService>> _loggerMock;
    private readonly LocalFileStorageService _service;

    public LocalFileStorageServiceTests()
    {
        // Create a temporary directory for testing
        _testBasePath = Path.Combine(Path.GetTempPath(), $"FileStorageTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testBasePath);

        // Set environment variables
        Environment.SetEnvironmentVariable(EnvironmentVariables.SheetsFolderPath, _testBasePath);
        // Disable soft-delete by default to maintain backward compatibility with existing tests
        Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, "1");

        _hostEnvironmentMock = new Mock<IHostEnvironment>();
        _hostEnvironmentMock.Setup(x => x.ContentRootPath).Returns(Path.GetTempPath());

        _loggerMock = new Mock<ILogger<LocalFileStorageService>>();

        _service = new LocalFileStorageService(_hostEnvironmentMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
        Environment.SetEnvironmentVariable(EnvironmentVariables.SheetsFolderPath, null);
        Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, null);
        Environment.SetEnvironmentVariable(EnvironmentVariables.TrashFolderPath, null);
    }

    [Fact]
    public async Task SaveFileAsync_WithValidStream_SavesFile()
    {
        // Arrange
        var fileName = "test.pdf";
        var content = "Test content"u8.ToArray();
        var stream = new MemoryStream(content);

        // Act
        var result = await _service.SaveFileAsync(stream, fileName);

        // Assert
        Assert.Equal(fileName, result);
        var filePath = Path.Combine(_testBasePath, fileName);
        Assert.True(File.Exists(filePath));
        var savedContent = await File.ReadAllBytesAsync(filePath, TestContext.Current.CancellationToken);
        Assert.Equal(content, savedContent);
    }

    [Fact]
    public async Task SaveFileAsync_WithNullStream_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.SaveFileAsync(null!, "test.pdf"));
    }

    [Fact]
    public async Task SaveFileAsync_WithEmptyFileName_ThrowsArgumentException()
    {
        // Arrange
        var stream = new MemoryStream([1, 2, 3]);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.SaveFileAsync(stream, ""));
    }

    [Fact]
    public async Task SaveFileAsync_WithNullFileName_ThrowsArgumentException()
    {
        // Arrange
        var stream = new MemoryStream([1, 2, 3]);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _service.SaveFileAsync(stream, null!));
    }

    [Fact]
    public async Task SaveFileAsync_WithUnreadableStream_ThrowsArgumentException()
    {
        // Arrange
        var stream = new Mock<Stream>();
        stream.Setup(s => s.CanRead).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.SaveFileAsync(stream.Object, "test.pdf"));
    }

    [Fact]
    public async Task SaveFileAsync_OverwritesExistingFile()
    {
        // Arrange
        var fileName = "overwrite.pdf";
        var originalContent = "Original"u8.ToArray();
        var newContent = "New content"u8.ToArray();
        
        await _service.SaveFileAsync(new MemoryStream(originalContent), fileName);

        // Act
        await _service.SaveFileAsync(new MemoryStream(newContent), fileName);

        // Assert
        var filePath = Path.Combine(_testBasePath, fileName);
        var savedContent = await File.ReadAllBytesAsync(filePath, TestContext.Current.CancellationToken);
        Assert.Equal(newContent, savedContent);
    }

    [Fact]
    public void FileExists_WithExistingFile_ReturnsTrue()
    {
        // Arrange
        var fileName = "exists.pdf";
        var filePath = Path.Combine(_testBasePath, fileName);
        File.WriteAllText(filePath, "test");

        // Act
        var result = _service.FileExists(fileName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void FileExists_WithNonExistingFile_ReturnsFalse()
    {
        // Act
        var result = _service.FileExists("nonexistent.pdf");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteFile_WithExistingFile_DeletesFile()
    {
        // Arrange
        var fileName = "todelete.pdf";
        var filePath = Path.Combine(_testBasePath, fileName);
        await File.WriteAllTextAsync(filePath, "test", TestContext.Current.CancellationToken);

        // Act
        await _service.DeleteFile(fileName, "test");

        // Assert
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task DeleteFile_WithNonExistingFile_ThrowsFileNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() => 
            _service.DeleteFile("nonexistent.pdf", "test"));
    }

    [Fact]
    public void RenameFile_WithExistingFile_RenamesFile()
    {
        // Arrange
        var oldFileName = "old.pdf";
        var newFileName = "new.pdf";
        var oldFilePath = Path.Combine(_testBasePath, oldFileName);
        var newFilePath = Path.Combine(_testBasePath, newFileName);
        File.WriteAllText(oldFilePath, "test");

        // Act
        _service.RenameFile(oldFileName, newFileName);

        // Assert
        Assert.False(File.Exists(oldFilePath));
        Assert.True(File.Exists(newFilePath));
    }

    [Fact]
    public void RenameFile_WithNonExistingFile_ThrowsFileNotFoundException()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => 
            _service.RenameFile("nonexistent.pdf", "new.pdf"));
    }

    [Fact]
    public void RenameFile_OverwritesExistingDestination()
    {
        // Arrange
        var oldFileName = "source.pdf";
        var newFileName = "destination.pdf";
        var oldFilePath = Path.Combine(_testBasePath, oldFileName);
        var newFilePath = Path.Combine(_testBasePath, newFileName);
        File.WriteAllText(oldFilePath, "source content");
        File.WriteAllText(newFilePath, "destination content");

        // Act
        _service.RenameFile(oldFileName, newFileName);

        // Assert
        Assert.False(File.Exists(oldFilePath));
        Assert.True(File.Exists(newFilePath));
        var content = File.ReadAllText(newFilePath);
        Assert.Equal("source content", content);
    }

    [Fact]
    public void GetSafeFilePath_WithValidFileName_ReturnsFullPath()
    {
        // Arrange
        var fileName = "test.pdf";

        // Act
        var result = _service.GetSafeFilePath(fileName);

        // Assert
        Assert.Equal(Path.Combine(_testBasePath, fileName), result);
    }

    [Fact]
    public void GetSafeFilePath_WithEmptyFileName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.GetSafeFilePath(""));
    }

    [Fact]
    public void GetSafeFilePath_WithNullFileName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.GetSafeFilePath(null!));
    }

    [Fact]
    public void GetSafeFilePath_WithWhitespaceFileName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.GetSafeFilePath("   "));
    }

    [Fact]
    public void GetSafeFilePath_WithPathTraversal_SafelyExtractsFileName()
    {
        // Arrange
        var pathWithTraversal = "../../../etc/passwd";

        // Act
        var result = _service.GetSafeFilePath(pathWithTraversal);

        // Assert - Path.GetFileName strips the traversal, leaving just "passwd"
        Assert.Equal(Path.Combine(_testBasePath, "passwd"), result);
        Assert.True(result.StartsWith(_testBasePath));
    }

    [Fact]
    public void GetSafeFilePath_WithFileNameContainingPath_ExtractsFileName()
    {
        // Arrange
        var fileNameWithPath = Path.Combine("subfolder", "test.pdf");

        // Act
        var result = _service.GetSafeFilePath(fileNameWithPath);

        // Assert
        Assert.Equal(Path.Combine(_testBasePath, "test.pdf"), result);
    }

    [Fact]
    public void Constructor_WithoutEnvironmentVariable_ThrowsInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable(EnvironmentVariables.SheetsFolderPath, null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            new LocalFileStorageService(_hostEnvironmentMock.Object, _loggerMock.Object));

        // Restore for cleanup
        Environment.SetEnvironmentVariable(EnvironmentVariables.SheetsFolderPath, _testBasePath);
        Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, "1");
    }

    [Fact]
    public void Constructor_WithRelativePath_CreatesDirectoryRelativeToContentRoot()
    {
        // Arrange
        var relativePath = "TestSheets";
        var expectedPath = Path.Combine(Path.GetTempPath(), relativePath);
        Environment.SetEnvironmentVariable(EnvironmentVariables.SheetsFolderPath, relativePath);

        try
        {
            // Act
            var service = new LocalFileStorageService(_hostEnvironmentMock.Object, _loggerMock.Object);

            // Assert
            Assert.True(Directory.Exists(expectedPath));

            // Cleanup
            Directory.Delete(expectedPath, true);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvironmentVariables.SheetsFolderPath, _testBasePath);
            Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, "1");
        }
    }

    [Fact]
    public void Constructor_WithAbsolutePath_UsesAbsolutePath()
    {
        // Arrange
        var absolutePath = Path.Combine(Path.GetTempPath(), $"AbsoluteTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(absolutePath);
        Environment.SetEnvironmentVariable(EnvironmentVariables.SheetsFolderPath, absolutePath);

        try
        {
            // Act
            var service = new LocalFileStorageService(_hostEnvironmentMock.Object, _loggerMock.Object);

            // Assert - should use the absolute path directly
            Assert.True(Directory.Exists(absolutePath));

            // Cleanup
            Directory.Delete(absolutePath, true);
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvironmentVariables.SheetsFolderPath, _testBasePath);
            Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, "1");
        }
    }

    [Fact]
    public async Task DeleteFile_WithSoftDeleteEnabled_MovesFileToTrash()
    {
        // Arrange
        var trashPath = Path.Combine(_testBasePath, "trash");
        Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, null);
        var service = new LocalFileStorageService(_hostEnvironmentMock.Object, _loggerMock.Object);

        var fileName = "softdelete.pdf";
        var filePath = Path.Combine(_testBasePath, fileName);
        await File.WriteAllTextAsync(filePath, "test content", TestContext.Current.CancellationToken);

        try
        {
            // Act
            await service.DeleteFile(fileName, "update");

            // Assert
            Assert.False(File.Exists(filePath));
            Assert.True(Directory.Exists(trashPath));
            var trashFiles = Directory.GetFiles(trashPath, "softdelete_*_update.pdf");
            Assert.Single(trashFiles);
        }
        finally
        {
            if (Directory.Exists(trashPath))
            {
                Directory.Delete(trashPath, true);
            }
            Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, "1");
        }
    }

    [Fact]
    public async Task DeleteFile_WithSoftDeleteEnabled_AppendsTimestampAndReason()
    {
        // Arrange
        var trashPath = Path.Combine(_testBasePath, "trash");
        Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, null);
        var service = new LocalFileStorageService(_hostEnvironmentMock.Object, _loggerMock.Object);

        var fileName = "timestamp_test.pdf";
        var filePath = Path.Combine(_testBasePath, fileName);
        await File.WriteAllTextAsync(filePath, "test content", TestContext.Current.CancellationToken);

        try
        {
            // Act
            await service.DeleteFile(fileName, "delete");

            // Assert
            var trashFiles = Directory.GetFiles(trashPath);
            Assert.Single(trashFiles);
            var trashFileName = Path.GetFileName(trashFiles[0]);
            Assert.StartsWith("timestamp_test_", trashFileName);
            Assert.EndsWith("_delete.pdf", trashFileName);
            // Verify timestamp format (14 digits between underscores)
            var parts = trashFileName.Split('_');
            Assert.Equal(4, parts.Length);
            Assert.Equal(14, parts[2].Length);
        }
        finally
        {
            if (Directory.Exists(trashPath))
            {
                Directory.Delete(trashPath, true);
            }
            Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, "1");
        }
    }

    [Fact]
    public async Task DeleteFile_WithExistingFileWithForcePermanent_DeletesFile()
    {
        // Arrange
        var fileName = "permanent_delete.pdf";
        var filePath = Path.Combine(_testBasePath, fileName);
        await File.WriteAllTextAsync(filePath, "test", TestContext.Current.CancellationToken);

        // Act
        await _service.DeleteFile(fileName, forcePermanent:true);

        // Assert
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void Constructor_WithCustomTrashPath_UsesCustomPath()
    {
        // Arrange
        var customTrashPath = Path.Combine(Path.GetTempPath(), $"CustomTrash_{Guid.NewGuid()}");
        Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, null);
        Environment.SetEnvironmentVariable(EnvironmentVariables.TrashFolderPath, customTrashPath);

        try
        {
            // Act
            var service = new LocalFileStorageService(_hostEnvironmentMock.Object, _loggerMock.Object);

            // Assert
            Assert.True(Directory.Exists(customTrashPath));
        }
        finally
        {
            if (Directory.Exists(customTrashPath))
            {
                Directory.Delete(customTrashPath, true);
            }
            Environment.SetEnvironmentVariable(EnvironmentVariables.TrashFolderPath, null);
            Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, "1");
        }
    }

    [Fact]
    public void Constructor_WithSoftDeleteDisabled_DoesNotCreateTrashDirectory()
    {
        // Arrange
        var trashPath = Path.Combine(_testBasePath, "trash");
        Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, "1");

        try
        {
            // Act
            var service = new LocalFileStorageService(_hostEnvironmentMock.Object, _loggerMock.Object);

            // Assert
            Assert.False(Directory.Exists(trashPath));
        }
        finally
        {
            Environment.SetEnvironmentVariable(EnvironmentVariables.SoftDeleteDisabled, "1");
        }
    }

    [Fact]
    public async Task DeleteFile_WithSoftDeleteDisabled_PermanentlyDeletesFile()
    {
        // Arrange
        var trashPath = Path.Combine(_testBasePath, "trash");
        var fileName = "permanent.pdf";
        var filePath = Path.Combine(_testBasePath, fileName);
        await File.WriteAllTextAsync(filePath, "test", TestContext.Current.CancellationToken);

        // Act
        await _service.DeleteFile(fileName, "update");

        // Assert
        Assert.False(File.Exists(filePath));
        Assert.False(Directory.Exists(trashPath));
    }
}