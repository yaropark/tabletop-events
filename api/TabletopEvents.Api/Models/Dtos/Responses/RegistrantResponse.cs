namespace TabletopEvents.Api.Models.Dtos.Responses;

public sealed class RegistrantResponse
{
    public required string Name { get; init; }
    public required DateTime RegisteredUtc { get; init; }
}
