using System.Net;
using BalsisNoteSheetLibrary.Server.Application.Interfaces;
using BalsisNoteSheetLibrary.Server.Application.Services;
using BalsisNoteSheetLibrary.Server.Domain.Interfaces;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.DbContext;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.Repositories;
using BalsisNoteSheetLibrary.Server.Infrastructure.Data.Seeders;
using BalsisNoteSheetLibrary.Server.Infrastructure.Hubs;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services;
using BalsisNoteSheetLibrary.Server.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// Dev only services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure routing
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Configure Identity
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

// Configure authentication and authorization
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

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());


// Setup logging
builder.Services.AddLogging();

// Configure antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "XSRF-TOKEN";
    options.HeaderName = "X-CSRF-TOKEN";
});

builder.Services.AddControllersWithViews(options =>
{
    //options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddCors(options =>
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

builder.Services.AddSignalR();

builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<INoteSheetRepository, NoteSheetRepository>();
builder.Services.AddScoped<INoteSheetService, NoteSheetService>();
builder.Services.AddScoped<INoteSheetRenameService, NoteSheetRenameService>();



var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseHttpsRedirection();


await RoleSeeder.SeedRolesAsync(app.Services);
await UserSeeder.SeedUsersAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    app.UseCors("DisableCSRFAndAuthForLocalhost");
}
else
{
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();
}

app.MapControllers();
app.MapHub<StatusHub>("/api/statusHub");

//Fallback to 404
app.MapFallback((context =>
{
    context.Response.StatusCode = 404;

    return context.Response.CompleteAsync();
}));

app.Run();