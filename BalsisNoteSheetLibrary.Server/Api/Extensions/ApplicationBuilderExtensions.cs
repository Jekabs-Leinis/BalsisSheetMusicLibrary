using BalsisNoteSheetLibrary.Server.Api.Middleware;
using BalsisNoteSheetLibrary.Server.Infrastructure.Hubs;

namespace BalsisNoteSheetLibrary.Server.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder ConfigurePipeline(this IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

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
            app.UseAntiforgery();
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

        return app;
    }
}