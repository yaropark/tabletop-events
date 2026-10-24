using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using TabletopEvents.Api.Data;
using TabletopEvents.Api.Options;
using TabletopEvents.Api.Repositories.Contracts;
using TabletopEvents.Api.Repositories.Implementations;
using TabletopEvents.Api.Services.Contracts;
using TabletopEvents.Api.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
    // Report validation errors under JSON names ("name"), matching the request body and the UI form fields.
    options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider()));
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<StoreOptions>(builder.Configuration.GetSection("Store"));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // Behind the optional tunnel, trust X-Forwarded-Proto/Host so links use the public https address.
    // A spoofed header only changes the links in the spoofer's own response.
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Infrastructure
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddSingleton<IStoreClock, StoreClock>();
builder.Services.AddSingleton<IPublicUrlService, PublicUrlService>();

// Repositories
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();

// Services
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<ICalendarInviteService, CalendarInviteService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<DemoDataSeeder>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    DatabaseInitializer.Initialize(services.GetRequiredService<DbConnectionFactory>());
    services.GetRequiredService<ITemplateService>().SyncFromFile(
        Path.Combine(app.Environment.ContentRootPath, app.Configuration["TemplatesFile"] ?? "game-templates.json"));
    if (app.Configuration.GetValue("SeedDemoData", true))
        services.GetRequiredService<DemoDataSeeder>().SeedIfEmpty();
}

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
// Unknown /api routes stay 404 instead of falling through to the Angular app.
app.Map("/api/{**path}", () => Results.NotFound());
// Client-side routes such as /register/7 (the QR code target) are served by the Angular app.
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
