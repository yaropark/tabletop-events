using Dapper;
using Microsoft.Extensions.Options;
using TabletopEvents.Api.Data;
using TabletopEvents.Api.Templates;

namespace TabletopEvents.Api.Events;

public sealed class EventService(
    Database db, TemplateRepository templates, StoreClock clock, IOptions<StoreOptions> store, PublicUrls urls)
{
    private const string SelectEvents = """
        SELECT e.Id, e.TemplateKey, e.Name, e.Format, e.StartUtc, e.DurationMinutes, e.Capacity,
               e.MinPlayers, e.Location, t.Name AS GameName, t.Color,
               (SELECT COUNT(*) FROM Registrations r WHERE r.EventId = e.Id) AS RegisteredCount
        FROM Events e
        JOIN GameTemplates t ON t.Key = e.TemplateKey
        """;

    public (EventDetails? Event, Dictionary<string, string[]> Errors) Create(CreateEventRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        void Fail(string field, string message) => errors[field] = [message];

        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name) || name.Length > 120)
            Fail("name", "Name is required (max 120 characters).");

        var template = string.IsNullOrWhiteSpace(request.TemplateKey) ? null : templates.Get(request.TemplateKey);
        if (template is null)
        {
            Fail("templateKey", "Choose a valid game.");
            return (null, errors);
        }

        // Every game-specific value comes from the template; nothing here knows which game it is.
        var format = string.IsNullOrWhiteSpace(request.Format) ? template.Formats[0] : request.Format.Trim();
        if (!template.Formats.Contains(format))
            Fail("format", $"{template.Name} supports: {string.Join(", ", template.Formats)}.");

        var duration = request.DurationMinutes ?? template.DefaultDurationMinutes;
        if (duration is < 15 or > 24 * 60)
            Fail("durationMinutes", "Duration must be between 15 minutes and 24 hours.");

        var capacity = request.Capacity ?? template.DefaultCapacity;
        if (capacity < template.MinPlayers || capacity > template.MaxCapacity)
            Fail("capacity", $"Capacity for {template.Name} must be between {template.MinPlayers} and {template.MaxCapacity}.");

        DateTime startUtc = default;
        if (request.StartLocal is not { } startLocal)
            Fail("startLocal", "Date and start time are required.");
        else if (clock.TimeZone.IsInvalidTime(DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified)))
            Fail("startLocal", "That time does not exist in the store's time zone (daylight saving change).");
        else if ((startUtc = clock.ToUtc(startLocal)) <= clock.UtcNow)
            Fail("startLocal", "Start time must be in the future.");

        var location = string.IsNullOrWhiteSpace(request.Location) ? store.Value.Address : request.Location.Trim();

        if (errors.Count > 0)
            return (null, errors);

        using var connection = db.Open();
        var id = connection.ExecuteScalar<long>("""
            INSERT INTO Events (TemplateKey, Name, Format, StartUtc, DurationMinutes, Capacity, MinPlayers, Location, CreatedUtc)
            VALUES (@TemplateKey, @Name, @Format, @StartUtc, @DurationMinutes, @Capacity, @MinPlayers, @Location, @CreatedUtc);
            SELECT last_insert_rowid();
            """,
            new
            {
                TemplateKey = template.Key,
                Name = name,
                Format = format,
                StartUtc = Database.FormatUtc(startUtc),
                DurationMinutes = duration,
                Capacity = capacity,
                template.MinPlayers,
                Location = location,
                CreatedUtc = Database.FormatUtc(clock.UtcNow),
            });

        return (Get((int)id), errors);
    }

    public EventDetails? Get(int id)
    {
        using var connection = db.Open();
        var row = connection.QuerySingleOrDefault<EventRow>($"{SelectEvents} WHERE e.Id = @id", new { id });
        return row is null ? null : ToDetails(row);
    }

    public IReadOnlyList<EventSummary> List(DateTime? fromLocal, DateTime? toLocal)
    {
        var from = fromLocal is { } f ? Database.FormatUtc(clock.ToUtc(f)) : "";
        var to = toLocal is { } t ? Database.FormatUtc(clock.ToUtc(t)) : "9999";

        using var connection = db.Open();
        return connection
            .Query<EventRow>($"{SelectEvents} WHERE e.StartUtc >= @from AND e.StartUtc < @to ORDER BY e.StartUtc", new { from, to })
            .Select(r =>
            {
                var (startLocal, endLocal, _, _) = Times(r);
                return new EventSummary((int)r.Id, r.Name, r.TemplateKey, r.GameName, r.Color, r.Format,
                    startLocal, endLocal, (int)r.Capacity, (int)r.RegisteredCount);
            })
            .ToList();
    }

    private EventDetails ToDetails(EventRow r)
    {
        var (startLocal, endLocal, startUtc, endUtc) = Times(r);
        var seatsLeft = Math.Max(0, (int)(r.Capacity - r.RegisteredCount));
        return new EventDetails(
            (int)r.Id, r.Name, r.TemplateKey, r.GameName, r.Color, r.Format,
            startLocal, endLocal, startUtc, endUtc, store.Value.TimeZone,
            (int)r.DurationMinutes, r.Location, (int)r.Capacity, (int)r.MinPlayers, (int)r.RegisteredCount,
            seatsLeft,
            RegistrationOpen: seatsLeft > 0 && startUtc > clock.UtcNow,
            urls.RegistrationUrl((int)r.Id));
    }

    private (DateTime StartLocal, DateTime EndLocal, DateTime StartUtc, DateTime EndUtc) Times(EventRow r)
    {
        var startUtc = Database.ParseUtc(r.StartUtc);
        var endUtc = startUtc.AddMinutes(r.DurationMinutes);
        return (clock.ToLocal(startUtc), clock.ToLocal(endUtc), startUtc, endUtc);
    }

    private sealed class EventRow
    {
        public long Id { get; set; }
        public string TemplateKey { get; set; } = "";
        public string Name { get; set; } = "";
        public string Format { get; set; } = "";
        public string StartUtc { get; set; } = "";
        public long DurationMinutes { get; set; }
        public long Capacity { get; set; }
        public long MinPlayers { get; set; }
        public string Location { get; set; } = "";
        public string GameName { get; set; } = "";
        public string Color { get; set; } = "";
        public long RegisteredCount { get; set; }
    }
}
