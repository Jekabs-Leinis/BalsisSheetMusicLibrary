using System.Net;
using System.Threading.RateLimiting;
using BalsisSheetMusicLibrary.Server.Application.Interfaces;
using BalsisSheetMusicLibrary.Server.Application.Services;
using BalsisSheetMusicLibrary.Server.Domain.Interfaces;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.DbContext;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.Repositories;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.UnitOfWork;
using BalsisSheetMusicLibrary.Server.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BalsisSheetMusicLibrary.Server.Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        RegisterDb(builder);
        builder.Services.AddDataProtection()
            .PersistKeysToDbContext<AppDbContext>();
        RegisterAuthentication(builder);
        builder.Services.AddAuthorizationBuilder();
        RegisterAntiforgery(builder);
        RegisterRateLimiting(builder);

        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddSerilog((services, lc) =>
        {
            lc.ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services);
        });

        // Dev only services
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        RegisterAppServices(builder);
    }

    private static void RegisterAntiforgery(WebApplicationBuilder builder)
    {
        builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-TOKEN"; });
    }

    private static void RegisterAppServices(WebApplicationBuilder builder)
    {
        // Infrastructure services
        builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
        // Repositories
        builder.Services.AddScoped<ISheetMusicRepository, SheetMusicRepository>();
        builder.Services.AddScoped<ISetListRepository, SetListRepository>();
        builder.Services.AddScoped<ISetListItemRepository, SetListItemRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        // Application services
        builder.Services.AddScoped<ISheetMusicService, SheetMusicMusicService>();
        builder.Services.AddScoped<ISheetMusicRenameService, SheetMusicMusicRenameService>();
        builder.Services.AddScoped<ISetListService, SetListService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ISetListItemService, SetListItemService>();
        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddHttpContextAccessor();
    }

    private static void RegisterAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
            });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/login";
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.Headers.Location = context.RedirectUri;

                    return Task.CompletedTask;
                }
            };
        });
    }

    private static void RegisterDb(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
    }

    private static void RegisterRateLimiting(WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("auth", opt =>
            {
                opt.PermitLimit = 20;
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 0;
            });
        });
    }
}