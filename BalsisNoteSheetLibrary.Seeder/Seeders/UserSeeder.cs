using BalsisNoteSheetLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BalsisNoteSheetLibrary.Seeder.Seeders;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        Console.WriteLine("Seeding users...");
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var adminPassword = Environment.GetEnvironmentVariable("LIB_ADMIN_PASS") ?? throw new InvalidOperationException("LIB_ADMIN_PASS environment variable is not set");
        var userPassword = Environment.GetEnvironmentVariable("LIB_USER_PASS") ?? throw new InvalidOperationException("LIB_USER_PASS environment variable is not set");
        
        var allowPasswordReset = Environment.GetEnvironmentVariable("ALLOW_USER_PASSWORD_RESET") == "1";

        await SeedUserAsync(userManager, "admin@balsis.lv", Role.Admin, adminPassword, allowPasswordReset);
        await SeedUserAsync(userManager, "daudzasbalsis", Role.User, userPassword, allowPasswordReset);
        Console.WriteLine("Seeding users completed!");
    }

    private static async Task SeedUserAsync(UserManager<IdentityUser> userManager, string userName, string role, string password, bool allowPasswordReset)
    {
        var user = await userManager.FindByNameAsync(userName);

        if (user == null)
        {
            var newUser = new IdentityUser
            {
                UserName = userName
            };
            var result = await userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                Console.WriteLine($"Created user: {newUser.UserName}");
                await userManager.AddToRoleAsync(newUser, role);
                Console.WriteLine($"Assigned role '{role}' to user: {newUser.UserName}");
            }
            else
            {
                Console.WriteLine(
                    $"Error creating user {newUser.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            if (allowPasswordReset)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, password);
                
                if (result.Succeeded)
                {
                    Console.WriteLine($"Updated password for user: {user.UserName}");
                }
                else
                {
                    Console.WriteLine($"Failed to update password for user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine($"Skipping password reset for user {user.UserName} in production environment.");
            }
            
            // Ensure user has the role if they already exist
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
                Console.WriteLine($"Assigned role '{role}' to existing user: {user.UserName}");
            }
        }    
    }
}