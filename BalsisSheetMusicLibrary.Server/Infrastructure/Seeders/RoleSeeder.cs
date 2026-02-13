using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Serilog;
using ILogger = Serilog.ILogger;

namespace BalsisSheetMusicLibrary.Server.Infrastructure.Seeders;

public static class RoleSeeder
{
        private static readonly ILogger Logger = Log.ForContext(typeof(RoleSeeder));
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        Logger.Information("Seeding roles...");
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        string[] roleNames = [Role.Admin, Role.User];

        foreach (var roleName in roleNames)
        {
            var roleExists = await roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                Logger.Information("Creating role: {RoleName}", roleName);
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        
        Logger.Information("Seeding roles completed!");
    }
}