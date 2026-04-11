using BalsisSheetMusicLibrary.Server.Api.Extensions;
using BalsisSheetMusicLibrary.Server.Infrastructure.Data.DbContext;
using Serilog;
using Serilog.Events;
using dotenv.net;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    DotEnv.Load();
    
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.Configure<RouteOptions>(options =>
    {
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = true;
    });

    builder.AddApplicationServices();

    var app = builder.Build();
    
    if (!args.Contains("skip-migrate"))
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }

    await app.ConfigurePipeline();

    app.Run();
}
catch (Exception ex) when (ex is HostAbortedException)
{
    // This is expected when running EF migrations, do not log as fatal
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}