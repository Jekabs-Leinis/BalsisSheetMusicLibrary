using BalsisNoteSheetLibrary.Server.Application.DTOs.User;

namespace BalsisNoteSheetLibrary.Server.Application.Interfaces;

public interface IUserService
{
    public Task<List<UserDto>> GetAllUsersAsync();
}