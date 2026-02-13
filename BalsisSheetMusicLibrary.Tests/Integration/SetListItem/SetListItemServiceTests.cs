using BalsisSheetMusicLibrary.Server.Application.DTOs.SetList;
using BalsisSheetMusicLibrary.Server.Application.Services;
using Entities = BalsisSheetMusicLibrary.Server.Domain.Entities;

namespace BalsisSheetMusicLibrary.Tests.Integration.SetListItem;

public class SetListItemServiceTests : IntegrationTestBase
{
    private const uint NonexistentSetListId = 9999;
    private const uint NonexistentSheetMusicId = 8888;
    private readonly SetListItemService _service;

    public SetListItemServiceTests()
    {
        _service = new SetListItemService(UnitOfWork);
        // Clean up test data
        UnitOfWork.SetListItems.RemoveRange(UnitOfWork.SetListItems.GetAllAsync().Result);
        UnitOfWork.SetLists.RemoveRange(UnitOfWork.SetLists.GetAsync().Result);
        UnitOfWork.SaveChangesAsync().Wait();
    }

    [Fact]
    public async Task MoveSetListItemAsync_SetListNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new MoveSetListItemDto { SetListId = NonexistentSetListId, SheetMusicId = 1, NewOrder = 0 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MoveSetListItemAsync(dto));
    }

    [Fact]
    public async Task MoveSetListItemAsync_ItemNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var setList = new Entities.SetList { Title = "Test List" };
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var dto = new MoveSetListItemDto
            { SetListId = setList.Id!.Value, SheetMusicId = NonexistentSheetMusicId, NewOrder = 0 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MoveSetListItemAsync(dto));
    }

    [Fact]
    public async Task MoveSetListItemAsync_ValidMove_UpdatesOrderCorrectly()
    {
        // Arrange
        var setList = new Entities.SetList { Title = "List" };
        UnitOfWork.SetLists.Add(setList);
        var SheetMusic = new List<Entities.SheetMusic>
        {
            new() { Title = "A" },
            new() { Title = "B" },
            new() { Title = "C" }
        };
        UnitOfWork.SheetMusic.AddRange(SheetMusic);

        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var setListId = setList.Id!.Value;
        var items = new List<Entities.SetListItem>
        {
            new() { SetListId = setListId, SheetMusicId = SheetMusic[0].Id, Order = 0 },
            new() { SetListId = setListId, SheetMusicId = SheetMusic[1].Id, Order = 1 },
            new() { SetListId = setListId, SheetMusicId = SheetMusic[2].Id, Order = 2 }
        };
        UnitOfWork.SetListItems.AddRange(items);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var dto = new MoveSetListItemDto { SetListId = setListId, SheetMusicId = SheetMusic[2].Id!.Value, NewOrder = 0 };

        // Act
        await _service.MoveSetListItemAsync(dto);
        var updatedItems = (await UnitOfWork.SetListItems.GetAllAsync()).Where(i => i.SetListId == setListId)
            .OrderBy(i => i.Order).ToList();

        // Assert
        Assert.Equal(3u, updatedItems[0].SheetMusicId); // moved to first
        Assert.Equal(1u, updatedItems[1].SheetMusicId);
        Assert.Equal(2u, updatedItems[2].SheetMusicId);
        Assert.Equal(0u, updatedItems[0].Order);
        Assert.Equal(1u, updatedItems[1].Order);
        Assert.Equal(2u, updatedItems[2].Order);
    }

    [Fact]
    public async Task MoveSetListItemAsync_MoveToLastPosition_UpdatesOrderCorrectly()
    {
        // Arrange
        var setList = new Entities.SetList { Title = "List" };
        UnitOfWork.SetLists.Add(setList);
        var SheetMusic = new List<Entities.SheetMusic>
        {
            new() { Title = "A" },
            new() { Title = "B" },
            new() { Title = "C" }
        };
        UnitOfWork.SheetMusic.AddRange(SheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var setListId = setList.Id!.Value;
        var items = new List<Entities.SetListItem>
        {
            new() { SetListId = setListId, SheetMusicId = SheetMusic[0].Id, Order = 0 },
            new() { SetListId = setListId, SheetMusicId = SheetMusic[1].Id, Order = 1 },
            new() { SetListId = setListId, SheetMusicId = SheetMusic[2].Id, Order = 2 }
        };
        UnitOfWork.SetListItems.AddRange(items);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var dto = new MoveSetListItemDto { SetListId = setListId, SheetMusicId = SheetMusic[0].Id!.Value, NewOrder = 2 };

        // Act
        await _service.MoveSetListItemAsync(dto);
        var updatedItems = (await UnitOfWork.SetListItems.GetAllAsync()).Where(i => i.SetListId == setListId)
            .OrderBy(i => i.Order).ToList();

        // Assert
        Assert.Equal(SheetMusic[1].Id, updatedItems[0].SheetMusicId); // B
        Assert.Equal(SheetMusic[2].Id, updatedItems[1].SheetMusicId); // C
        Assert.Equal(SheetMusic[0].Id, updatedItems[2].SheetMusicId); // A moved to last
    }

    [Fact]
    public async Task MoveSetListItemAsync_MoveToSamePosition_NoOrderChange()
    {
        // Arrange
        var setList = new Entities.SetList { Title = "List" };
        UnitOfWork.SetLists.Add(setList);
        var SheetMusic = new List<Entities.SheetMusic>
        {
            new() { Title = "A" },
            new() { Title = "B" },
            new() { Title = "C" }
        };
        UnitOfWork.SheetMusic.AddRange(SheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var setListId = setList.Id!.Value;
        var items = new List<Entities.SetListItem>
        {
            new() { SetListId = setListId, SheetMusicId = SheetMusic[0].Id, Order = 0 },
            new() { SetListId = setListId, SheetMusicId = SheetMusic[1].Id, Order = 1 },
            new() { SetListId = setListId, SheetMusicId = SheetMusic[2].Id, Order = 2 }
        };
        UnitOfWork.SetListItems.AddRange(items);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var dto = new MoveSetListItemDto { SetListId = setListId, SheetMusicId = SheetMusic[1].Id!.Value, NewOrder = 1 };

        // Act
        await _service.MoveSetListItemAsync(dto);
        var updatedItems = (await UnitOfWork.SetListItems.GetAllAsync()).Where(i => i.SetListId == setListId)
            .OrderBy(i => i.Order).ToList();

        // Assert
        Assert.Equal(SheetMusic[0].Id, updatedItems[0].SheetMusicId); // A
        Assert.Equal(SheetMusic[1].Id, updatedItems[1].SheetMusicId); // B (same position)
        Assert.Equal(SheetMusic[2].Id, updatedItems[2].SheetMusicId); // C
    }

    [Fact]
    public async Task MoveSetListItemAsync_MoveToOutOfRangePosition_MovesToEnd()
    {
        // Arrange
        var setList = new Entities.SetList { Title = "List" };
        UnitOfWork.SetLists.Add(setList);
        var SheetMusic = new List<Entities.SheetMusic>
        {
            new() { Title = "A" },
            new() { Title = "B" },
            new() { Title = "C" }
        };
        UnitOfWork.SheetMusic.AddRange(SheetMusic);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var setListId = setList.Id!.Value;
        var items = new List<Entities.SetListItem>
        {
            new() { SetListId = setListId, SheetMusicId = SheetMusic[0].Id, Order = 0 },
            new() { SetListId = setListId, SheetMusicId = SheetMusic[1].Id, Order = 1 },
            new() { SetListId = setListId, SheetMusicId = SheetMusic[2].Id, Order = 2 }
        };
        UnitOfWork.SetListItems.AddRange(items);
        await UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        var dto = new MoveSetListItemDto
            { SetListId = setListId, SheetMusicId = SheetMusic[0].Id!.Value, NewOrder = 99 }; // out of range

        // Act
        await _service.MoveSetListItemAsync(dto);
        var updatedItems = (await UnitOfWork.SetListItems.GetAllAsync()).Where(i => i.SetListId == setListId)
            .OrderBy(i => i.Order).ToList();

        // Assert
        Assert.Equal(SheetMusic[1].Id, updatedItems[0].SheetMusicId); // B
        Assert.Equal(SheetMusic[2].Id, updatedItems[1].SheetMusicId); // C
        Assert.Equal(SheetMusic[0].Id, updatedItems[2].SheetMusicId); // A moved to last
    }
}