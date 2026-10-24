namespace TabletopEvents.Api.Models.Dtos.Responses;

/// <summary>Everything the event and registration pages show: the summary plus details and seat status.</summary>
public sealed class EventDetailsResponse : EventSummaryResponse
{
    public required DateTime StartUtc { get; init; }
    public required DateTime EndUtc { get; init; }
    public required string TimeZone { get; init; }
    public required int DurationMinutes { get; init; }
    public required string Location { get; init; }
    public required int MinPlayers { get; init; }
    public required int SeatsLeft { get; init; }
    public required bool RegistrationOpen { get; init; }
    public required string RegistrationUrl { get; init; }
}
