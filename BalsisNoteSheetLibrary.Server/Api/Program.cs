using BalsisNoteSheetLibrary.Server.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.ConfigurePipeline();

app.Run();