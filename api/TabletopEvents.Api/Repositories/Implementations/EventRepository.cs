using Dapper;
using TabletopEvents.Api.Data;
using TabletopEvents.Api.Models.Entities;
using TabletopEvents.Api.Repositories.Contracts;

namespace TabletopEvents.Api.Repositories.Implementations;

public sealed class EventRepository(DbConnectionFactory db) : IEventRepository
{
    private const string SelectEvents = """
        SELECT e.Id, e.TemplateKey, e.Name, e.Format, e.StartUtc, e.DurationMinutes, e.Capacity,
               e.MinPlayers, e.Location, e.CreatedUtc
        FROM Events e
        """;

    private const string SelectOverviews = """
        SELECT e.Id, e.TemplateKey, e.Name, e.Format, e.StartUtc, e.DurationMinutes, e.Capacity,
               e.MinPlayers, e.Location, e.CreatedUtc, t.Name AS GameName, t.Color AS GameColor,
               (SELECT COUNT(*) FROM Registrations r WHERE r.EventId = e.Id) AS RegisteredCount
        FROM Events e
        JOIN GameTemplates t ON t.Key = e.TemplateKey
        """;

    public int Insert(Event evt)
    {
        using var connection = db.Open();
        return (int)connection.ExecuteScalar<long>("""
            INSERT INTO Events (TemplateKey, Name, Format, StartUtc, DurationMinutes, Capacity, MinPlayers, Location, CreatedUtc)
            VALUES (@TemplateKey, @Name, @Format, @StartUtc, @DurationMinutes, @Capacity, @MinPlayers, @Location, @CreatedUtc);
            SELECT last_insert_rowid();
            """,
            new
            {
                evt.TemplateKey,
                evt.Name,
                evt.Format,
                StartUtc = SqliteDates.Format(evt.StartUtc),
                evt.DurationMinutes,
                evt.Capacity,
                evt.MinPlayers,
                evt.Location,
                CreatedUtc = SqliteDates.Format(evt.CreatedUtc),
            });
    }

    public Event? Get(int id)
    {
        using var connection = db.Open();
        var row = connection.QuerySingleOrDefault<EventRow>($"{SelectEvents} WHERE e.Id = @id", new { id });
        return row is null ? null : ToEntity(row);
    }

    public EventOverview? GetOverview(int id)
    {
        using var connection = db.Open();
        var row = connection.QuerySingleOrDefault<EventRow>($"{SelectOverviews} WHERE e.Id = @id", new { id });
        return row is null ? null : ToOverview(row);
    }

    public IReadOnlyList<EventOverview> ListOverviews(DateTime? fromUtc, DateTime? toUtc)
    {
        var from = fromUtc is { } f ? SqliteDates.Format(f) : "";
        var to = toUtc is { } t ? SqliteDates.Format(t) : "9999";

        using var connection = db.Open();
        return connection
            .Query<EventRow>($"{SelectOverviews} WHERE e.StartUtc >= @from AND e.StartUtc < @to ORDER BY e.StartUtc", new { from, to })
            .Select(ToOverview)
            .ToList();
    }

    public bool Any()
    {
        using var connection = db.Open();
        return connection.ExecuteScalar<bool>("SELECT EXISTS (SELECT 1 FROM Events)");
    }

    private static Event ToEntity(EventRow r) => new()
    {
        Id = (int)r.Id,
        TemplateKey = r.TemplateKey,
        Name = r.Name,
        Format = r.Format,
        StartUtc = SqliteDates.Parse(r.StartUtc),
        DurationMinutes = (int)r.DurationMinutes,
        Capacity = (int)r.Capacity,
        MinPlayers = (int)r.MinPlayers,
        Location = r.Location,
        CreatedUtc = SqliteDates.Parse(r.CreatedUtc),
    };

    private static EventOverview ToOverview(EventRow r) => new()
    {
        Event = ToEntity(r),
        GameName = r.GameName,
        GameColor = r.GameColor,
        RegisteredCount = (int)r.RegisteredCount,
    };

    /// <summary>Column-shaped row for Dapper; the overview columns are empty for plain event queries.</summary>
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
        public string CreatedUtc { get; set; } = "";
        public string GameName { get; set; } = "";
        public string GameColor { get; set; } = "";
        public long RegisteredCount { get; set; }
    }
}
