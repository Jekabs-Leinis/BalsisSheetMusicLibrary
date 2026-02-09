namespace BalsisNoteSheetLibrary.Tests.Integration.Repository;

public class BaseRepositoryTests : IntegrationTestBase
{
    [Fact]
    public async Task GetByKeysAsync_WithValidKeys_ReturnsEntity()
    {
        // Arrange
        var noteSheet = new Server.Domain.Entities.NoteSheet { Id = 1, Title = "Test" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await UnitOfWork.NoteSheets.GetByKeysAsync(1u);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(noteSheet.Title, result.Title);
    }

    [Fact]
    public async Task GetByKeysAsync_WithInvalidKeys_ReturnsNull()
    {
        // Act
        var result = await UnitOfWork.NoteSheets.GetByKeysAsync(999u);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WithFilter_ReturnsFilteredEntities()
    {
        // Arrange
        UnitOfWork.NoteSheets.AddRange([
            new Server.Domain.Entities.NoteSheet { Title = "A" },
            new Server.Domain.Entities.NoteSheet { Title = "B" },
            new Server.Domain.Entities.NoteSheet { Title = "C" }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await UnitOfWork.NoteSheets.GetAsync(filter: ns => ns.Title == "B");

        // Assert
        Assert.Single(result);
        Assert.Equal("B", result[0].Title);
    }

    [Fact]
    public async Task GetAsync_WithOrderByDescending_ReturnsOrderedEntities()
    {
        // Arrange
        UnitOfWork.NoteSheets.AddRange([
            new Server.Domain.Entities.NoteSheet { Title = "A" },
            new Server.Domain.Entities.NoteSheet { Title = "C" },
            new Server.Domain.Entities.NoteSheet { Title = "B" }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await UnitOfWork.NoteSheets.GetAsync(
            orderBy: ns => ns.Title,
            orderByDescending: true
        );

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("C", result[0].Title);
        Assert.Equal("B", result[1].Title);
        Assert.Equal("A", result[2].Title);
    }

    [Fact]
    public async Task GetAsync_WithIncludeProperties_IncludesRelatedEntities()
    {
        // Arrange
        var noteSheet = new Server.Domain.Entities.NoteSheet { Id = 1, Title = "Test" };
        var setList = new Server.Domain.Entities.SetList { Id = 1, Title = "My Set" };
        var setListItem = new Server.Domain.Entities.SetListItem
        {
            SetListId = 1,
            NoteSheetId = 1,
            Order = 1
        };
        
        UnitOfWork.NoteSheets.Add(noteSheet);
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        UnitOfWork.SetListItems.Add(setListItem);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await UnitOfWork.SetListItems.GetAsync(
            includeProperties: ["NoteSheet", "SetList"]
        );

        // Assert
        Assert.Single(result);
        Assert.NotNull(result[0].NoteSheet);
        Assert.NotNull(result[0].SetList);
        Assert.Equal("Test", result[0].NoteSheet.Title);
        Assert.Equal("My Set", result[0].SetList.Title);
    }

    [Fact]
    public async Task GetAsync_WithNoTracking_ReturnsDetachedEntities()
    {
        // Arrange
        var noteSheet = new Server.Domain.Entities.NoteSheet { Id = 1, Title = "Test" };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await UnitOfWork.NoteSheets.GetAsync(withTracking: false);

        // Assert
        Assert.Single(result);
        // Verify entity is not tracked
        var entry = DbContext.Entry(result[0]);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Detached, entry.State);
    }
}
