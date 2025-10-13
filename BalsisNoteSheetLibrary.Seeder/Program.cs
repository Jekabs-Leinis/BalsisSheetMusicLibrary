using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using BalsisNoteSheetLibrary.Server.Infrastructure.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using dotenv.net;

DotEnv.Load();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var basePath = Directory.GetParent(context.HostingEnvironment.ContentRootPath)!.FullName;
        var databasePath = Path.Combine(basePath,"BalsisNoteSheetLibrary.Server", "app.db");
        var connectionString = $"Data Source={databasePath}";
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));
        
        services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
    })
    .Build();

try
{
    // Seed data
    Console.WriteLine("Seeding database...");
    await RoleSeeder.SeedRolesAsync(host.Services);
    await UserSeeder.SeedUsersAsync(host.Services);
    Console.WriteLine("✅ Seeding complete");
    
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex);

    return 1;
}

