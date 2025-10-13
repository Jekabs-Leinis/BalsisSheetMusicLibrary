using BalsisNoteSheetLibrary.Server.Api.Middleware;
using BalsisNoteSheetLibrary.Server.Infrastructure.Hubs;
using BalsisNoteSheetLibrary.Server.Infrastructure.Seeders;

namespace BalsisNoteSheetLibrary.Server.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static async Task ConfigurePipeline(this IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        var env = app.ApplicationServices.GetService<IHostEnvironment>();

        if (env != null && env.IsDevelopment())
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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHub<StatusHub>("/api/statusHub");
            endpoints.MapFallback(context =>
            {
                context.Response.StatusCode = 404;

                return context.Response.CompleteAsync();
            });
        });

        if (Environment.GetEnvironmentVariable("LIB_ENABLE_SEEDERS") == "1")
        {
            await RoleSeeder.SeedRolesAsync(app.ApplicationServices);
            await UserSeeder.SeedUsersAsync(app.ApplicationServices);
        }
    }
}