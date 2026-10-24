namespace TabletopEvents.Api.Templates;

/// <summary>
/// A game is pure data: everything the event logic needs to know about a game comes from here.
/// </summary>
public sealed record GameTemplate(
    string Key,
    string Name,
    string Category,
    string Color,
    IReadOnlyList<string> Formats,
    int DefaultDurationMinutes,
    int DefaultCapacity,
    int MaxCapacity,
    int MinPlayers);
