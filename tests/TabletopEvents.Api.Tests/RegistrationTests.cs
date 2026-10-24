using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using TabletopEvents.Api.Events;

namespace TabletopEvents.Api.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"tabletop-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_dbPath}");
        builder.UseSetting("SeedDemoData", "false");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        foreach (var suffix in new[] { "", "-wal", "-shm" })
            File.Delete(_dbPath + suffix);
    }
}

public sealed class RegistrationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Concurrent_registrations_for_the_last_seats_never_overbook()
    {
        var eventId = await CreateEvent(capacity: 10);

        var responses = await Task.WhenAll(Enumerable.Range(1, 40).Select(i =>
            _client.PostAsJsonAsync($"/api/events/{eventId}/registrations", new { name = $"Player {i}" })));

        Assert.Equal(10, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.Equal(30, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        var details = await _client.GetFromJsonAsync<EventDetails>($"/api/events/{eventId}");
        Assert.Equal(10, details!.RegisteredCount);
        Assert.Equal(0, details.SeatsLeft);
    }

    [Fact]
    public async Task Full_event_rejects_registration_with_clear_message()
    {
        var eventId = await CreateEvent(capacity: 4);
        foreach (var name in new[] { "Ann", "Bo", "Cy", "Di" })
            (await Register(eventId, name)).EnsureSuccessStatusCode();

        var response = await Register(eventId, "Eve");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Sorry, this event is full.", problem!.Detail);
    }

    [Fact]
    public async Task Duplicate_registration_is_rejected_by_email_or_by_name()
    {
        var eventId = await CreateEvent(capacity: 10);

        Assert.Equal(HttpStatusCode.Created, (await Register(eventId, "Ava Martin")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await Register(eventId, "  ava   MARTIN ")).StatusCode);

        // With an email, the email is the identity: same name is fine, same email is not.
        Assert.Equal(HttpStatusCode.Created, (await Register(eventId, "Ava Martin", "ava@example.com")).StatusCode);
        var duplicate = await Register(eventId, "Someone Else", "AVA@example.com");
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal("You're already registered for this event.",
            (await duplicate.Content.ReadFromJsonAsync<ProblemDetails>())!.Detail);
    }

    private async Task<int> CreateEvent(int capacity)
    {
        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            name = "Test event",
            templateKey = "swu",
            startLocal = DateTime.Today.AddDays(7).AddHours(18).ToString("yyyy-MM-ddTHH:mm"),
            capacity,
        });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<EventDetails>())!.Id;
    }

    private Task<HttpResponseMessage> Register(int eventId, string name, string? email = null) =>
        _client.PostAsJsonAsync($"/api/events/{eventId}/registrations", new { name, email });
}
