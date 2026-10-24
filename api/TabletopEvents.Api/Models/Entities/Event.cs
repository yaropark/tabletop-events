namespace TabletopEvents.Api.Models.Entities;

/// <summary>
/// A scheduled event. Capacity, duration, format and min players are copied from the game template
/// when the event is created, so later template edits never change existing events.
/// </summary>
public sealed class Event
{
    /// <summary>Store-wide hard limit; templates may set a lower maximum.</summary>
    public const int MaxCapacity = 30;

    public int Id { get; init; }
    public required string TemplateKey { get; init; }
    public required string Name { get; init; }
    public required string Format { get; init; }
    public required DateTime StartUtc { get; init; }
    public required int DurationMinutes { get; init; }
    public required int Capacity { get; init; }
    public required int MinPlayers { get; init; }
    public required string Location { get; init; }
    public required DateTime CreatedUtc { get; init; }

    public DateTime EndUtc => StartUtc.AddMinutes(DurationMinutes);
}
