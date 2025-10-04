using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace BalsisNoteSheetLibrary.Server.Infrastructure.Data.Seeders;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = [Role.Admin, Role.User];

        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                Console.WriteLine($"Creating role: {roleName}");
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}