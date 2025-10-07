using System.Net;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Application.Services;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BalsisNoteSheetLibrary.Server.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        RegisterDb(services, configuration);
        RegisterAuthentication(services);
        RegisterAuthorization(services);
        RegisterAntiforgery(services);

        services.AddControllers();
        services.AddSignalR();
        services.AddLogging();

        // Dev only services
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        RegisterAppServices(services);

        return services;
    }

    private static void RegisterAntiforgery(IServiceCollection services)
    {
        services.AddAntiforgery(options =>
        {
            options.Cookie.Name = "XSRF-TOKEN";
            options.HeaderName = "X-CSRF-TOKEN";
        });

        services.AddCors(options =>
        {
            options.AddPolicy("DisableCSRFAndAuthForLocalhost",
                b =>
                {
                    b.WithOrigins("https://localhost:7171")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });
    }

    private static void RegisterAppServices(IServiceCollection services)
    {
        // Infrastructure services
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        // Repositories
        services.AddScoped<INoteSheetRepository, NoteSheetRepository>();
        services.AddScoped<ISetListRepository, SetListRepository>();
        // Application services
        services.AddScoped<INoteSheetService, NoteSheetService>();
        services.AddScoped<INoteSheetRenameService, NoteSheetRenameService>();
        services.AddScoped<ISetListService, SetListService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISetListItemService, SetListItemService>();
    }

    private static void RegisterAuthorization(IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build());
    }

    private static void RegisterAuthentication(IServiceCollection services)
    {
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

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/login";
            });

        services.ConfigureApplicationCookie(options =>
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

    private static void RegisterDb(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
    }
}