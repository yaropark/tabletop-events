namespace TabletopEvents.Api.Events;

public static class EventRules
{
    /// <summary>Store-wide hard limit; templates may set a lower maximum.</summary>
    public const int MaxCapacity = 30;
}

/// <param name="StartLocal">Wall-clock start time in the store's time zone, e.g. 2026-10-30T18:00.</param>
/// <param name="Format">Optional; defaults to the template's first format.</param>
/// <param name="DurationMinutes">Optional; defaults to the template's duration.</param>
/// <param name="Capacity">Optional; defaults to the template's default capacity.</param>
/// <param name="Location">Optional; defaults to the store address.</param>
public sealed record CreateEventRequest(
    string? Name,
    string? TemplateKey,
    DateTime? StartLocal,
    string? Format,
    int? DurationMinutes,
    int? Capacity,
    string? Location);

public sealed record EventSummary(
    int Id,
    string Name,
    string TemplateKey,
    string GameName,
    string Color,
    string Format,
    DateTime StartLocal,
    DateTime EndLocal,
    int Capacity,
    int RegisteredCount);

public sealed record EventDetails(
    int Id,
    string Name,
    string TemplateKey,
    string GameName,
    string Color,
    string Format,
    DateTime StartLocal,
    DateTime EndLocal,
    DateTime StartUtc,
    DateTime EndUtc,
    string TimeZone,
    int DurationMinutes,
    string Location,
    int Capacity,
    int MinPlayers,
    int RegisteredCount,
    int SeatsLeft,
    bool RegistrationOpen,
    string RegistrationUrl);
