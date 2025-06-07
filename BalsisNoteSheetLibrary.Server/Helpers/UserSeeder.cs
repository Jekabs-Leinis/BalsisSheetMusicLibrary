using BalsisNoteSheetLibrary.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BalsisNoteSheetLibrary.Server.Helpers;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await SeedUserAsync(userManager, "admin@balsis.lv", Role.Admin);
        await SeedUserAsync(userManager, "daudzasbalsis", Role.User);
    }

    private static async Task SeedUserAsync(UserManager<IdentityUser> userManager, string userName, string role)
    {
        var user = await userManager.FindByNameAsync(userName);

        if (user == null)
        {
            var newUser = new IdentityUser
            {
                UserName = userName,
            };
            var result = await userManager.CreateAsync(newUser, "changeme");
            if (result.Succeeded)
            {
                Console.WriteLine($"Created user: {newUser.UserName}");
                await userManager.AddToRoleAsync(newUser, role);
                Console.WriteLine($"Assigned role '{role}' to user: {newUser.UserName}");
            }
            else
            {
                Console.WriteLine($"Error creating user {newUser.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Ensure user has the role if they already exist
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
                Console.WriteLine($"Assigned role '{role}' to existing user: {user.UserName}");
            }
        }
    }
}