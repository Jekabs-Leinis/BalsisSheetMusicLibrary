using BalsisSheetMusicLibrary.Server.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Serilog;
using ILogger = Serilog.ILogger;

namespace BalsisSheetMusicLibrary.Server.Infrastructure.Seeders;

public static class UserSeeder
{
    private static readonly ILogger Logger = Log.ForContext(typeof(UserSeeder));
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        Logger.Information("Seeding users...");
        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        var adminUserName = Environment.GetEnvironmentVariable(EnvironmentVariables.AdminUsername) ?? throw new InvalidOperationException(
            $"{EnvironmentVariables.AdminUsername} environment variable must be set to seed users");
        var adminPassword = Environment.GetEnvironmentVariable(EnvironmentVariables.AdminPassword) ?? throw new InvalidOperationException( $"{EnvironmentVariables.AdminPassword} environment variable must be set to seed users");
        
        var userUserName = Environment.GetEnvironmentVariable(EnvironmentVariables.UserUsername) ?? throw new InvalidOperationException($"{EnvironmentVariables.UserUsername} environment variable must be set to seed users");
        var userPassword = Environment.GetEnvironmentVariable(EnvironmentVariables.UserPassword) ?? throw new InvalidOperationException($"{EnvironmentVariables.UserPassword} environment variable must be set to seed users");
        
        await SeedUserAsync(userManager, adminUserName, Role.Admin, adminPassword);
        await SeedUserAsync(userManager, userUserName, Role.User, userPassword);
        Logger.Information("Seeding users completed!");
    }

    private static async Task SeedUserAsync(UserManager<IdentityUser> userManager, string userName, string role, string password)
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
                Logger.Information("Created user: {NewUserUserName}", newUser.UserName);
                await userManager.AddToRoleAsync(newUser, role);
                Logger.Information("Assigned role '{Role}' to user: {NewUserUserName}", role, newUser.UserName);
            }
            else
            {
                Logger.Information(
                    "Error creating user {NewUserUserName}: {Join}", newUser.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            if (Environment.GetEnvironmentVariable(EnvironmentVariables.AllowSeederPasswordReset) == "1")
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, password);
                
                if (result.Succeeded)
                {
                    Logger.Information("Updated password for user: {UserUserName}", user.UserName);
                }
                else
                {
                                        Logger.Error("Failed to update password for user {UserUserName}: {Join}", user.UserName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                Logger.Information("Skipping password reset for user {UserUserName} in production environment.", user.UserName);
            }
            
            // Ensure user has the role if they already exist
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
                Logger.Information("Assigned role '{Role}' to existing user: {UserUserName}", role, user.UserName);
            }
        }    
    }
}