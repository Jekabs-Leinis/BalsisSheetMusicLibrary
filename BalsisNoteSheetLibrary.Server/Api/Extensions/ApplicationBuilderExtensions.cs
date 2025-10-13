using BalsisNoteSheetLibrary.Server.Api.Middleware;
using BalsisNoteSheetLibrary.Server.Infrastructure.Hubs;
using BalsisNoteSheetLibrary.Server.Infrastructure.Seeders;
using Serilog;

namespace BalsisNoteSheetLibrary.Server.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task ConfigurePipeline(this WebApplication app)
    {
        app.UseDefaultFiles();
        app.UseStaticFiles();
        
        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            app.UseHttpsRedirection();
        }

        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.MapControllers().RequireAuthorization();
        
        app.MapHub<StatusHub>("/api/statusHub");
        
        // Return 404 for any /api/* requests that don't match a controller
        // This prevents serving index.html for unknown API routes
        // which would otherwise result in a 200 OK with HTML content
        app.MapFallback(context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = 404;

                return context.Response.CompleteAsync();
            }
            
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html";
            
            return context.Response.SendFileAsync("wwwroot/index.html");
        });

        if (Environment.GetEnvironmentVariable("LIB_ENABLE_SEEDERS") == "1")
        {
            await RoleSeeder.SeedRolesAsync(app.Services);
            await UserSeeder.SeedUsersAsync(app.Services);
        }
    }
}