using BalsisSheetMusicLibrary.Server.Api.Extensions;
using Serilog;
using Serilog.Events;
using dotenv.net;

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