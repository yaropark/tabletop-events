namespace TabletopEvents.Api.Models.Entities;

/// <summary>Read model: an event joined with its game's display data and its current seat count.</summary>
public sealed class EventOverview
{
    public required Event Event { get; init; }
    public required string GameName { get; init; }
    public required string GameColor { get; init; }
    public required int RegisteredCount { get; init; }
}
