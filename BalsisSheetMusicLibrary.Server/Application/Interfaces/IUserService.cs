using BalsisSheetMusicLibrary.Server.Application.DTOs.User;

namespace BalsisSheetMusicLibrary.Server.Application.Interfaces;

public interface IUserService
{
    public Task<List<UserDto>> GetAllUsersAsync();
}