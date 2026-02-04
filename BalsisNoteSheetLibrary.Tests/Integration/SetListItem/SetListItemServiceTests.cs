using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;
using BalsisNoteSheetLibrary.Server.Application.Services;
using Entities = BalsisNoteSheetLibrary.Server.Domain.Entities;

namespace BalsisNoteSheetLibrary.Tests.Integration.SetListItem;

public class SetListItemServiceTests : IntegrationTestBase
{
    private const uint NonexistentSetListId = 9999;
    private const uint NonexistentNoteSheetId = 8888;
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
        var dto = new MoveSetListItemDto { SetListId = NonexistentSetListId, NoteSheetId = 1, NewOrder = 0 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MoveSetListItemAsync(dto));
    }

    [Fact]
    public async Task MoveSetListItemAsync_ItemNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var setList = new Entities.SetList { Title = "Test List" };
        UnitOfWork.SetLists.Add(setList);
        await UnitOfWork.SaveChangesAsync();
        var dto = new MoveSetListItemDto
            { SetListId = setList.Id!.Value, NoteSheetId = NonexistentNoteSheetId, NewOrder = 0 };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MoveSetListItemAsync(dto));
    }

    [Fact]
    public async Task MoveSetListItemAsync_ValidMove_UpdatesOrderCorrectly()
    {
        // Arrange
        var setList = new Entities.SetList { Title = "List" };
        UnitOfWork.SetLists.Add(setList);
        var noteSheets = new List<Entities.NoteSheet>
        {
            new() { Title = "A" },
            new() { Title = "B" },
            new() { Title = "C" }
        };
        UnitOfWork.NoteSheets.AddRange(noteSheets);

        await UnitOfWork.SaveChangesAsync();
        var setListId = setList.Id!.Value;
        var items = new List<Entities.SetListItem>
        {
            new() { SetListId = setListId, NoteSheetId = noteSheets[0].Id, Order = 0 },
            new() { SetListId = setListId, NoteSheetId = noteSheets[1].Id, Order = 1 },
            new() { SetListId = setListId, NoteSheetId = noteSheets[2].Id, Order = 2 }
        };
        UnitOfWork.SetListItems.AddRange(items);
        await UnitOfWork.SaveChangesAsync();
        var dto = new MoveSetListItemDto { SetListId = setListId, NoteSheetId = noteSheets[2].Id!.Value, NewOrder = 0 };

        // Act
        await _service.MoveSetListItemAsync(dto);
        var updatedItems = (await UnitOfWork.SetListItems.GetAllAsync()).Where(i => i.SetListId == setListId)
            .OrderBy(i => i.Order).ToList();

        // Assert
        Assert.Equal(3u, updatedItems[0].NoteSheetId); // moved to first
        Assert.Equal(1u, updatedItems[1].NoteSheetId);
        Assert.Equal(2u, updatedItems[2].NoteSheetId);
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
        var noteSheets = new List<Entities.NoteSheet>
        {
            new() { Title = "A" },
            new() { Title = "B" },
            new() { Title = "C" }
        };
        UnitOfWork.NoteSheets.AddRange(noteSheets);
        await UnitOfWork.SaveChangesAsync();
        var setListId = setList.Id!.Value;
        var items = new List<Entities.SetListItem>
        {
            new() { SetListId = setListId, NoteSheetId = noteSheets[0].Id, Order = 0 },
            new() { SetListId = setListId, NoteSheetId = noteSheets[1].Id, Order = 1 },
            new() { SetListId = setListId, NoteSheetId = noteSheets[2].Id, Order = 2 }
        };
        UnitOfWork.SetListItems.AddRange(items);
        await UnitOfWork.SaveChangesAsync();
        var dto = new MoveSetListItemDto { SetListId = setListId, NoteSheetId = noteSheets[0].Id!.Value, NewOrder = 2 };

        // Act
        await _service.MoveSetListItemAsync(dto);
        var updatedItems = (await UnitOfWork.SetListItems.GetAllAsync()).Where(i => i.SetListId == setListId)
            .OrderBy(i => i.Order).ToList();

        // Assert
        Assert.Equal(noteSheets[1].Id, updatedItems[0].NoteSheetId); // B
        Assert.Equal(noteSheets[2].Id, updatedItems[1].NoteSheetId); // C
        Assert.Equal(noteSheets[0].Id, updatedItems[2].NoteSheetId); // A moved to last
    }

    [Fact]
    public async Task MoveSetListItemAsync_MoveToSamePosition_NoOrderChange()
    {
        // Arrange
        var setList = new Entities.SetList { Title = "List" };
        UnitOfWork.SetLists.Add(setList);
        var noteSheets = new List<Entities.NoteSheet>
        {
            new() { Title = "A" },
            new() { Title = "B" },
            new() { Title = "C" }
        };
        UnitOfWork.NoteSheets.AddRange(noteSheets);
        await UnitOfWork.SaveChangesAsync();
        var setListId = setList.Id!.Value;
        var items = new List<Entities.SetListItem>
        {
            new() { SetListId = setListId, NoteSheetId = noteSheets[0].Id, Order = 0 },
            new() { SetListId = setListId, NoteSheetId = noteSheets[1].Id, Order = 1 },
            new() { SetListId = setListId, NoteSheetId = noteSheets[2].Id, Order = 2 }
        };
        UnitOfWork.SetListItems.AddRange(items);
        await UnitOfWork.SaveChangesAsync();
        var dto = new MoveSetListItemDto { SetListId = setListId, NoteSheetId = noteSheets[1].Id!.Value, NewOrder = 1 };

        // Act
        await _service.MoveSetListItemAsync(dto);
        var updatedItems = (await UnitOfWork.SetListItems.GetAllAsync()).Where(i => i.SetListId == setListId)
            .OrderBy(i => i.Order).ToList();

        // Assert
        Assert.Equal(noteSheets[0].Id, updatedItems[0].NoteSheetId); // A
        Assert.Equal(noteSheets[1].Id, updatedItems[1].NoteSheetId); // B (same position)
        Assert.Equal(noteSheets[2].Id, updatedItems[2].NoteSheetId); // C
    }

    [Fact]
    public async Task MoveSetListItemAsync_MoveToOutOfRangePosition_MovesToEnd()
    {
        // Arrange
        var setList = new Entities.SetList { Title = "List" };
        UnitOfWork.SetLists.Add(setList);
        var noteSheets = new List<Entities.NoteSheet>
        {
            new() { Title = "A" },
            new() { Title = "B" },
            new() { Title = "C" }
        };
        UnitOfWork.NoteSheets.AddRange(noteSheets);
        await UnitOfWork.SaveChangesAsync();
        var setListId = setList.Id!.Value;
        var items = new List<Entities.SetListItem>
        {
            new() { SetListId = setListId, NoteSheetId = noteSheets[0].Id, Order = 0 },
            new() { SetListId = setListId, NoteSheetId = noteSheets[1].Id, Order = 1 },
            new() { SetListId = setListId, NoteSheetId = noteSheets[2].Id, Order = 2 }
        };
        UnitOfWork.SetListItems.AddRange(items);
        await UnitOfWork.SaveChangesAsync();
        var dto = new MoveSetListItemDto
            { SetListId = setListId, NoteSheetId = noteSheets[0].Id!.Value, NewOrder = 99 }; // out of range

        // Act
        await _service.MoveSetListItemAsync(dto);
        var updatedItems = (await UnitOfWork.SetListItems.GetAllAsync()).Where(i => i.SetListId == setListId)
            .OrderBy(i => i.Order).ToList();

        // Assert
        Assert.Equal(noteSheets[1].Id, updatedItems[0].NoteSheetId); // B
        Assert.Equal(noteSheets[2].Id, updatedItems[1].NoteSheetId); // C
        Assert.Equal(noteSheets[0].Id, updatedItems[2].NoteSheetId); // A moved to last
    }
}