using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BalsisNoteSheetLibrary.Seeder.Seeders;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Seeding roles...");
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
        
        Console.WriteLine("Seeding roles completed!");
    }
}