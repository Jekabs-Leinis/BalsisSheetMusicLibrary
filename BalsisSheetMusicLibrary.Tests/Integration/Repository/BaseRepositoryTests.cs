namespace BalsisSheetMusicLibrary.Tests.Integration.Repository;

public class BaseRepositoryTests : IntegrationTestBase
{
    [Fact]
    public async Task GetByKeysAsync_WithValidKeys_ReturnsEntity()
    {
        // Arrange
        var sheetMusic = new Server.Domain.Entities.SheetMusic { Id = 1, Title = "Test" };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await UnitOfWork.SheetMusic.GetByKeysAsync(1u);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(sheetMusic.Title, result.Title);
    }

    [Fact]
    public async Task GetByKeysAsync_WithInvalidKeys_ReturnsNull()
    {
        // Act
        var result = await UnitOfWork.SheetMusic.GetByKeysAsync(999u);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAsync_WithFilter_ReturnsFilteredEntities()
    {
        // Arrange
        UnitOfWork.SheetMusic.AddRange([
            new Server.Domain.Entities.SheetMusic { Title = "A" },
            new Server.Domain.Entities.SheetMusic { Title = "B" },
            new Server.Domain.Entities.SheetMusic { Title = "C" }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await UnitOfWork.SheetMusic.GetAsync(filter: ns => ns.Title == "B");

        // Assert
        Assert.Single(result);
        Assert.Equal("B", result[0].Title);
    }

    [Fact]
    public async Task GetAsync_WithOrderByDescending_ReturnsOrderedEntities()
    {
        // Arrange
        UnitOfWork.SheetMusic.AddRange([
            new Server.Domain.Entities.SheetMusic { Title = "A" },
            new Server.Domain.Entities.SheetMusic { Title = "C" },
            new Server.Domain.Entities.SheetMusic { Title = "B" }
        ]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await UnitOfWork.SheetMusic.GetAsync(
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
        var sheetMusic = new Server.Domain.Entities.SheetMusic { Id = 1, Title = "Test" };
        var setList = new Server.Domain.Entities.SetList { Id = 1, Title = "My Set" };
        var setListItem = new Server.Domain.Entities.SetListItem
        {
            SetListId = 1,
            SheetMusicId = 1,
            Order = 1
        };
        
        UnitOfWork.SheetMusic.Add(sheetMusic);
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        
        UnitOfWork.SetListItems.Add(setListItem);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await UnitOfWork.SetListItems.GetAsync(
            includeProperties: ["SheetMusic", "SetList"]
        );

        // Assert
        Assert.Single(result);
        Assert.NotNull(result[0].SheetMusic);
        Assert.NotNull(result[0].SetList);
        Assert.Equal("Test", result[0].SheetMusic.Title);
        Assert.Equal("My Set", result[0].SetList.Title);
    }

    [Fact]
    public async Task GetAsync_WithNoTracking_ReturnsDetachedEntities()
    {
        // Arrange
        var sheetMusic = new Server.Domain.Entities.SheetMusic { Id = 1, Title = "Test" };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await UnitOfWork.SheetMusic.GetAsync(withTracking: false);

        // Assert
        Assert.Single(result);
        // Verify entity is not tracked
        var entry = DbContext.Entry(result[0]);
        Assert.Equal(Microsoft.EntityFrameworkCore.EntityState.Detached, entry.State);
    }
}
