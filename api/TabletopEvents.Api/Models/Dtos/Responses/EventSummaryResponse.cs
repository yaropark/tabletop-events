namespace TabletopEvents.Api.Models.Dtos.Responses;

/// <summary>What the calendar needs per event. Base for <see cref="EventDetailsResponse"/>.</summary>
public class EventSummaryResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string TemplateKey { get; init; }
    public required string GameName { get; init; }
    public required string Color { get; init; }
    public required string Format { get; init; }
    /// <summary>Store wall-clock time without an offset, e.g. 2026-10-30T18:00:00.</summary>
    public required DateTime StartLocal { get; init; }
    public required DateTime EndLocal { get; init; }
    public required int Capacity { get; init; }
    public required int RegisteredCount { get; init; }
}
