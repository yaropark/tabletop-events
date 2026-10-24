namespace TabletopEvents.Api.Models.Entities;

/// <summary>
/// A game is pure data: everything the event logic needs to know about a game comes from here.
/// Loaded from game-templates.json and stored in the GameTemplates table.
/// </summary>
public sealed class GameTemplate
{
    public required string Key { get; init; }
    public required string Name { get; init; }
    public required string Category { get; init; }
    public required string Color { get; init; }
    public required IReadOnlyList<string> Formats { get; init; }
    public required int DefaultDurationMinutes { get; init; }
    public required int DefaultCapacity { get; init; }
    public required int MaxCapacity { get; init; }
    public required int MinPlayers { get; init; }
}
