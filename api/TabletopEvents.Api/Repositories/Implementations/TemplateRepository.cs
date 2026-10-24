using System.Text.Json;
using Dapper;
using TabletopEvents.Api.Data;
using TabletopEvents.Api.Models.Entities;
using TabletopEvents.Api.Repositories.Contracts;

namespace TabletopEvents.Api.Repositories.Implementations;

public sealed class TemplateRepository(DbConnectionFactory db) : ITemplateRepository
{
    private const string SelectColumns = """
        SELECT Key, Name, Category, Color, FormatsJson, DefaultDurationMinutes,
               DefaultCapacity, MaxCapacity, MinPlayers
        FROM GameTemplates
        """;

    public IReadOnlyList<GameTemplate> GetAll()
    {
        using var connection = db.Open();
        return connection.Query<TemplateRow>($"{SelectColumns} ORDER BY Name").Select(ToEntity).ToList();
    }

    public GameTemplate? Get(string key)
    {
        using var connection = db.Open();
        var row = connection.QuerySingleOrDefault<TemplateRow>($"{SelectColumns} WHERE Key = @key", new { key });
        return row is null ? null : ToEntity(row);
    }

    public void Upsert(IEnumerable<GameTemplate> templates)
    {
        using var connection = db.Open();
        using var transaction = connection.BeginTransaction();
        foreach (var t in templates)
        {
            connection.Execute("""
                INSERT INTO GameTemplates (Key, Name, Category, Color, FormatsJson, DefaultDurationMinutes,
                                           DefaultCapacity, MaxCapacity, MinPlayers)
                VALUES (@Key, @Name, @Category, @Color, @FormatsJson, @DefaultDurationMinutes,
                        @DefaultCapacity, @MaxCapacity, @MinPlayers)
                ON CONFLICT (Key) DO UPDATE SET
                    Name = excluded.Name, Category = excluded.Category, Color = excluded.Color,
                    FormatsJson = excluded.FormatsJson, DefaultDurationMinutes = excluded.DefaultDurationMinutes,
                    DefaultCapacity = excluded.DefaultCapacity, MaxCapacity = excluded.MaxCapacity,
                    MinPlayers = excluded.MinPlayers
                """,
                new
                {
                    t.Key, t.Name, t.Category, t.Color,
                    FormatsJson = JsonSerializer.Serialize(t.Formats),
                    t.DefaultDurationMinutes, t.DefaultCapacity, t.MaxCapacity, t.MinPlayers,
                },
                transaction);
        }
        transaction.Commit();
    }

    private static GameTemplate ToEntity(TemplateRow r) => new()
    {
        Key = r.Key,
        Name = r.Name,
        Category = r.Category,
        Color = r.Color,
        Formats = JsonSerializer.Deserialize<List<string>>(r.FormatsJson) ?? [],
        DefaultDurationMinutes = (int)r.DefaultDurationMinutes,
        DefaultCapacity = (int)r.DefaultCapacity,
        MaxCapacity = (int)r.MaxCapacity,
        MinPlayers = (int)r.MinPlayers,
    };

    /// <summary>Column-shaped row for Dapper; formats are stored as a JSON array.</summary>
    private sealed class TemplateRow
    {
        public string Key { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Color { get; set; } = "";
        public string FormatsJson { get; set; } = "[]";
        public long DefaultDurationMinutes { get; set; }
        public long DefaultCapacity { get; set; }
        public long MaxCapacity { get; set; }
        public long MinPlayers { get; set; }
    }
}
