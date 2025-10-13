using BalsisNoteSheetLibrary.Server.Application.DTOs.User;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Application.Services;

public class UserService(UserManager<IdentityUser> userManager) : IUserService
{
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await userManager.Users.ToListAsync();

        return users.Select(UserDto.FromEntity).ToList();
    }
}