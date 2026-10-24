using System.Text.Json;
using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Models.Entities;
using TabletopEvents.Api.Repositories.Contracts;
using TabletopEvents.Api.Services.Contracts;

namespace TabletopEvents.Api.Services.Implementations;

public sealed class TemplateService(ITemplateRepository templates) : ITemplateService
{
    public IReadOnlyList<GameTemplateResponse> GetAll() =>
        templates.GetAll().Select(t => new GameTemplateResponse
        {
            Key = t.Key,
            Name = t.Name,
            Category = t.Category,
            Color = t.Color,
            Formats = t.Formats,
            DefaultDurationMinutes = t.DefaultDurationMinutes,
            DefaultCapacity = t.DefaultCapacity,
            MaxCapacity = t.MaxCapacity,
            MinPlayers = t.MinPlayers,
        }).ToList();

    /// <summary>
    /// Runs on every startup. Templates removed from the file are kept because existing events still reference them.
    /// </summary>
    public void SyncFromFile(string path)
    {
        var fromFile = JsonSerializer.Deserialize<List<GameTemplate>>(File.ReadAllText(path), JsonSerializerOptions.Web) ?? [];
        foreach (var template in fromFile)
            Validate(template);

        templates.Upsert(fromFile);
    }

    private static void Validate(GameTemplate t)
    {
        string? error =
            string.IsNullOrWhiteSpace(t.Key) || string.IsNullOrWhiteSpace(t.Name) ? "key and name are required"
            : t.Formats is not { Count: > 0 } ? "at least one format is required"
            : t.DefaultDurationMinutes <= 0 ? "defaultDurationMinutes must be positive"
            : t.MaxCapacity > Event.MaxCapacity ? $"maxCapacity cannot exceed {Event.MaxCapacity}"
            : !(1 <= t.MinPlayers && t.MinPlayers <= t.DefaultCapacity && t.DefaultCapacity <= t.MaxCapacity)
                ? "expected 1 <= minPlayers <= defaultCapacity <= maxCapacity"
            : null;

        if (error is not null)
            throw new InvalidOperationException($"Invalid game template '{t.Key}': {error}.");
    }
}
