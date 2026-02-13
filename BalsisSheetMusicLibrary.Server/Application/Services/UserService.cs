using BalsisSheetMusicLibrary.Server.Application.DTOs.User;
using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BalsisSheetMusicLibrary.Server.Application.Services;

public class UserService(UserManager<IdentityUser> userManager) : IUserService
{
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await userManager.Users.ToListAsync();

        return users.Select(UserDto.FromEntity).ToList();
    }
}