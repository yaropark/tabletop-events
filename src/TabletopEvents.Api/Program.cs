using Microsoft.AspNetCore.HttpOverrides;
using TabletopEvents.Api;
using TabletopEvents.Api.Data;
using TabletopEvents.Api.Events;
using TabletopEvents.Api.Registrations;
using TabletopEvents.Api.Templates;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StoreOptions>(builder.Configuration.GetSection("Store"));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<StoreClock>();
builder.Services.AddSingleton<Database>();
builder.Services.AddSingleton<TemplateRepository>();
builder.Services.AddSingleton<EventService>();
builder.Services.AddSingleton<RegistrationService>();
builder.Services.AddSingleton<PublicUrls>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddProblemDetails();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // Behind the optional tunnel, trust X-Forwarded-Proto/Host so links use the public https address.
    // A spoofed header only changes the links in the spoofer's own response.
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.Services.GetRequiredService<Database>().Initialize();
app.Services.GetRequiredService<TemplateRepository>().SyncFromFile(
    Path.Combine(app.Environment.ContentRootPath, app.Configuration["TemplatesFile"] ?? "game-templates.json"));
if (app.Configuration.GetValue("SeedDemoData", true))
{
    DemoData.SeedIfEmpty(
        app.Services.GetRequiredService<Database>(),
        app.Services.GetRequiredService<EventService>(),
        app.Services.GetRequiredService<RegistrationService>(),
        app.Services.GetRequiredService<StoreClock>());
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapApi();
// Client-side routes such as /register/7 (the QR code target) are served by the Angular app.
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
