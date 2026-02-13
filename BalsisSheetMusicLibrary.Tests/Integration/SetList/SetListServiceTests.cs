using BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;
using BalsisSheetMusicLibrary.Server.Application.Services;
using Entities = BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Tests.Integration.SetList;

public class SetListServiceTests : IntegrationTestBase
{
    private const uint NonexistentId = 9999;
    private readonly SetListService _service;

    public SetListServiceTests()
    {
        _service = new SetListService(UnitOfWork);
        // Clean up SetLists and SetListItems before each test
        UnitOfWork.SetListItems.RemoveRange(UnitOfWork.SetListItems.GetAllAsync().Result);
        UnitOfWork.SetLists.RemoveRange(UnitOfWork.SetLists.GetAsync().Result);
        UnitOfWork.SaveChangesAsync().Wait();
    }

    [Fact]
    public async Task CreateSetListAsync_WithValidData_CreatesSetList()
    {
        // Arrange
        var dto = new CreateSetListDto { Title = "Test SetList" };

        // Act
        var result = await _service.CreateSetListAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.Title, result.Title);
        Assert.NotNull(result.Id);
    }

    [Fact]
    public async Task GetSetListByIdAsync_WithExistingId_ReturnsSetList()
    {
        // Arrange
        var dto = new CreateSetListDto { Title = "Test SetList" };
        var created = await _service.CreateSetListAsync(dto);

        // Act
        var result = await _service.GetSetListByIdAsync(created.Id!.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(dto.Title, result.Title);
    }

    [Fact]
    public async Task GetSetListByIdAsync_WithNonexistentId_ReturnsNull()
    {
        // Act
        var result = await _service.GetSetListByIdAsync(NonexistentId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateSetListAsync_WithValidData_UpdatesSetList()
    {
        // Arrange
        var created = await _service.CreateSetListAsync(new CreateSetListDto { Title = "Original Title" });
        var updateDto = new UpdateSetListDto
        {
            Id = created.Id!.Value,
            Title = "Updated Title",
            Items = new List<UpdateSetListItemDto>()
        };

        // Act
        var result = await _service.UpdateSetListAsync(updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Title, result.Title);
    }

    [Fact]
    public async Task DeleteSetListAsync_WithExistingId_DeletesSetList()
    {
        // Arrange
        var created = await _service.CreateSetListAsync(new CreateSetListDto { Title = "To Delete" });

        // Act
        await _service.DeleteSetListAsync(created.Id!.Value);
        var result = await _service.GetSetListByIdAsync(created.Id.Value);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ArchiveSetListAsync_WithExistingId_ArchivesSetList()
    {
        // Arrange
        var created = await _service.CreateSetListAsync(new CreateSetListDto { Title = "To Archive" });

        // Act
        await _service.ArchiveSetListAsync(created.Id!.Value);
        var result = await _service.GetSetListByIdAsync(created.Id.Value);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.ArchivedAt);
    }

    [Fact]
    public async Task RestoreSetListAsync_WithArchivedId_RestoresSetList()
    {
        // Arrange
        var created = await _service.CreateSetListAsync(new CreateSetListDto { Title = "To Restore" });
        await _service.ArchiveSetListAsync(created.Id!.Value);

        // Act
        await _service.RestoreSetListAsync(created.Id.Value);
        var result = await _service.GetSetListByIdAsync(created.Id.Value);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.ArchivedAt);
    }

    [Fact]
    public async Task MoveSetListAsync_WithValidData_ChangesOrder()
    {
        // Arrange
        var setList1 = await _service.CreateSetListAsync(new CreateSetListDto { Title = "First" });
        var setList2 = await _service.CreateSetListAsync(new CreateSetListDto { Title = "Second" });
        var setList3 = await _service.CreateSetListAsync(new CreateSetListDto { Title = "Third" });

        // Act
        await _service.MoveSetListAsync(new MoveSetListDto { Id = setList3.Id!.Value, NewOrder = 0 });
        var all = (await _service.GetAllSetListsAsync()).ToList();

        // Assert
        Assert.Equal(setList3.Id, all[0].Id);
        Assert.Equal(setList1.Id, all[1].Id);
        Assert.Equal(setList2.Id, all[2].Id);
    }

    [Fact]
    public async Task GetAllSetListsAsync_ReturnsAllSetLists()
    {
        // Arrange
        await _service.CreateSetListAsync(new CreateSetListDto { Title = "A" });
        await _service.CreateSetListAsync(new CreateSetListDto { Title = "B" });

        // Act
        var all = (await _service.GetAllSetListsAsync()).ToList();

        // Assert
        Assert.True(all.Count >= 2);
        Assert.Contains(all, s => s.Title == "A");
        Assert.Contains(all, s => s.Title == "B");
    }

    [Fact]
    public async Task GetAllArchivedSetListsAsync_ReturnsArchivedSetLists()
    {
        // Arrange
        var setList = await _service.CreateSetListAsync(new CreateSetListDto { Title = "To Archive" });
        await _service.ArchiveSetListAsync(setList.Id!.Value);

        // Act
        var archived = (await _service.GetAllArchivedSetListsAsync()).ToList();

        // Assert
        Assert.Contains(archived, s => s.Id == setList.Id);
    }

    [Fact]
    public async Task GetAllSetListsAsync_WithSheetMusicTrue_ReturnsSetListsWithItems()
    {
        // Arrange
        var setList = await _service.CreateSetListAsync(new CreateSetListDto { Title = "With Items" });
        var sheetMusic = new Entities.SheetMusic
        {
            Title = "SheetMusic 1"
        };
        UnitOfWork.SheetMusic.Add(sheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        // Add an item manually
        var item = new Entities.SetListItem { SetListId = setList.Id, SheetMusicId = sheetMusic.Id, Order = 0 };
        UnitOfWork.SetListItems.Add(item);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var all = (await _service.GetAllSetListsAsync(true)).ToList();

        // Assert
        var found = all.FirstOrDefault(s => s.Id == setList.Id);
        Assert.NotNull(found);
        Assert.NotEmpty(found.Items);
        Assert.Equal(1u, found.Items[0].SheetMusicId);
    }

    [Fact]
    public async Task UpdateSetListAsync_WithNonexistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        var updateDto = new UpdateSetListDto
        {
            Id = NonexistentId,
            Title = "Doesn't exist",
            Items = new List<UpdateSetListItemDto>()
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateSetListAsync(updateDto));
    }

    [Fact]
    public async Task UpdateSetListAsync_AddRemoveAndReorderItems_UpdatesCorrectly()
    {
        // Arrange
        var setList = await _service.CreateSetListAsync(new CreateSetListDto { Title = "With Items" });
        var sheetMusic1 = new Entities.SheetMusic { Title = "SheetMusic 1" };
        var sheetMusic2 = new Entities.SheetMusic { Title = "SheetMusic 2" };
        var sheetMusic3 = new Entities.SheetMusic
        {
            Title = "SheetMusic 3"
        };
        UnitOfWork.SheetMusic.AddRange([sheetMusic1, sheetMusic2, sheetMusic3]);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        var item1 = new Entities.SetListItem { SetListId = setList.Id!.Value, SheetMusicId = sheetMusic1.Id, Order = 0 };
        var item2 = new Entities.SetListItem { SetListId = setList.Id!.Value, SheetMusicId = sheetMusic2.Id, Order = 1 };
        UnitOfWork.SetListItems.Add(item1);
        UnitOfWork.SetListItems.Add(item2);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Remove item1, add item3, reorder item2
        var updateDto = new UpdateSetListDto
        {
            Id = setList.Id.Value,
            Title = "Updated",
            Items =
            [
                new UpdateSetListItemDto { SheetMusicId = sheetMusic2.Id!.Value, Order = 0 }, // item2 moved to first
                new UpdateSetListItemDto { SheetMusicId = sheetMusic3.Id!.Value, Order = 1 }
            ]
        };

        // Act
        var result = await _service.UpdateSetListAsync(updateDto);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2u, result.Items[0].SheetMusicId); // item2 first
        Assert.Equal(3u, result.Items[1].SheetMusicId); // new item
    }

    [Fact]
    public async Task DeleteSetListAsync_WithNonexistentId_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteSetListAsync(NonexistentId));
    }

    [Fact]
    public async Task MoveSetListAsync_WithNonexistentId_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.MoveSetListAsync(new MoveSetListDto { Id = NonexistentId, NewOrder = 0 }));
    }

    [Fact]
    public async Task ArchiveSetListAsync_WithNonexistentId_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ArchiveSetListAsync(NonexistentId));
    }

    [Fact]
    public async Task RestoreSetListAsync_WithNonexistentId_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RestoreSetListAsync(NonexistentId));
    }
}