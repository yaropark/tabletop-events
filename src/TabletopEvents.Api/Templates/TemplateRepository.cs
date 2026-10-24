using System.Text.Json;
using Dapper;
using TabletopEvents.Api.Data;
using TabletopEvents.Api.Events;

namespace TabletopEvents.Api.Templates;

public sealed class TemplateRepository(Database db)
{
    private const string SelectColumns = """
        SELECT Key, Name, Category, Color, FormatsJson, DefaultDurationMinutes,
               DefaultCapacity, MaxCapacity, MinPlayers
        FROM GameTemplates
        """;

    public IReadOnlyList<GameTemplate> GetAll()
    {
        using var connection = db.Open();
        return connection.Query<TemplateRow>($"{SelectColumns} ORDER BY Name").Select(ToModel).ToList();
    }

    public GameTemplate? Get(string key)
    {
        using var connection = db.Open();
        var row = connection.QuerySingleOrDefault<TemplateRow>($"{SelectColumns} WHERE Key = @key", new { key });
        return row is null ? null : ToModel(row);
    }

    /// <summary>
    /// Loads game-templates.json and upserts it on every startup, so adding a game is a data change only.
    /// Templates removed from the file are kept because existing events still reference them.
    /// </summary>
    public void SyncFromFile(string path)
    {
        var templates = JsonSerializer.Deserialize<List<GameTemplate>>(
            File.ReadAllText(path), JsonSerializerOptions.Web) ?? [];

        foreach (var template in templates)
            Validate(template);

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

    private static void Validate(GameTemplate t)
    {
        string? error =
            string.IsNullOrWhiteSpace(t.Key) || string.IsNullOrWhiteSpace(t.Name) ? "key and name are required"
            : t.Formats is not { Count: > 0 } ? "at least one format is required"
            : t.DefaultDurationMinutes <= 0 ? "defaultDurationMinutes must be positive"
            : t.MaxCapacity > EventRules.MaxCapacity ? $"maxCapacity cannot exceed {EventRules.MaxCapacity}"
            : !(1 <= t.MinPlayers && t.MinPlayers <= t.DefaultCapacity && t.DefaultCapacity <= t.MaxCapacity)
                ? "expected 1 <= minPlayers <= defaultCapacity <= maxCapacity"
            : null;

        if (error is not null)
            throw new InvalidOperationException($"Invalid game template '{t.Key}': {error}.");
    }

    private static GameTemplate ToModel(TemplateRow r) => new(
        r.Key, r.Name, r.Category, r.Color,
        JsonSerializer.Deserialize<List<string>>(r.FormatsJson) ?? [],
        (int)r.DefaultDurationMinutes, (int)r.DefaultCapacity, (int)r.MaxCapacity, (int)r.MinPlayers);

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
