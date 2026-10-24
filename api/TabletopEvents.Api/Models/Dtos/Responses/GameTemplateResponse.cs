namespace TabletopEvents.Api.Models.Dtos.Responses;

public sealed class GameTemplateResponse
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
