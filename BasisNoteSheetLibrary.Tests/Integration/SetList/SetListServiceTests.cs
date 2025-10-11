using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BalsisNoteSheetLibrary.Server.Application.DTOs.SetList;
using BalsisNoteSheetLibrary.Server.Application.Services;
using BalsisNoteSheetLibrary.Server.Domain.Entities;
using Xunit;

namespace BasisNoteSheetLibrary.Tests.Integration.SetList;

public class SetListServiceTests : IntegrationTestBase
{
    private const uint NonexistentId = 9999;
    private readonly SetListService _service;

    public SetListServiceTests()
    {
        _service = new SetListService(UnitOfWork);
        // Clean up SetLists and SetListItems before each test
        UnitOfWork.SetListItems.RemoveRange(UnitOfWork.SetListItems.GetAllAsync().Result);
        UnitOfWork.SetLists.RemoveRange(UnitOfWork.SetLists.GetAllAsync().Result);
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
    public async Task GetAllSetListsAsync_WithNoteSheetsTrue_ReturnsSetListsWithItems()
    {
        // Arrange
        var setList = await _service.CreateSetListAsync(new CreateSetListDto { Title = "With Items" });
        var noteSheet = new BalsisNoteSheetLibrary.Server.Domain.Entities.NoteSheet {
            Title = "NoteSheet 1"
        };
        UnitOfWork.NoteSheets.Add(noteSheet);
        await UnitOfWork.SaveChangesAsync();
        // Add an item manually
        var item = new SetListItem { SetListId = setList.Id, NoteSheetId = noteSheet.Id, Order = 0 };
        UnitOfWork.SetListItems.Add(item);
        await UnitOfWork.SaveChangesAsync();

        // Act
        var all = (await _service.GetAllSetListsAsync(true)).ToList();

        // Assert
        var found = all.FirstOrDefault(s => s.Id == setList.Id);
        Assert.NotNull(found);
        Assert.NotEmpty(found.Items);
        Assert.Equal(1u, found.Items[0].NoteSheetId);
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
        var noteSheet1 = new BalsisNoteSheetLibrary.Server.Domain.Entities.NoteSheet { Title = "NoteSheet 1" };
        var noteSheet2 = new BalsisNoteSheetLibrary.Server.Domain.Entities.NoteSheet { Title = "NoteSheet 2" };
        var noteSheet3 = new BalsisNoteSheetLibrary.Server.Domain.Entities.NoteSheet
        {
            Title = "NoteSheet 3"
        };
        UnitOfWork.NoteSheets.AddRange([noteSheet1, noteSheet2, noteSheet3]);
        await UnitOfWork.SaveChangesAsync();
        
        var item1 = new SetListItem { SetListId = setList.Id!.Value, NoteSheetId = noteSheet1.Id, Order = 0 };
        var item2 = new SetListItem { SetListId = setList.Id!.Value, NoteSheetId = noteSheet2.Id, Order = 1 };
        UnitOfWork.SetListItems.Add(item1);
        UnitOfWork.SetListItems.Add(item2);
        await UnitOfWork.SaveChangesAsync();

        // Remove item1, add item3, reorder item2
        var updateDto = new UpdateSetListDto
        {
            Id = setList.Id.Value,
            Title = "Updated",
            Items =
            [
                new UpdateSetListItemDto { NoteSheetId = noteSheet2.Id!.Value, Order = 0 }, // item2 moved to first
                new UpdateSetListItemDto { NoteSheetId = noteSheet3.Id!.Value , Order = 1 }
            ]
        };

        // Act
        var result = await _service.UpdateSetListAsync(updateDto);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2u, result.Items[0].NoteSheetId); // item2 first
        Assert.Equal(3u, result.Items[1].NoteSheetId); // new item
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
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MoveSetListAsync(new MoveSetListDto { Id = NonexistentId, NewOrder = 0 }));
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