namespace TabletopEvents.Api.Models.Dtos.Responses;

public sealed class RegistrationResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required int SeatsLeft { get; init; }
}
