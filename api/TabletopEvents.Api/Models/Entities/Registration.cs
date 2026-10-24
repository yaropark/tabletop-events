namespace TabletopEvents.Api.Models.Entities;

public sealed class Registration
{
    public int Id { get; init; }
    public required int EventId { get; init; }
    public required string Name { get; init; }
    /// <summary>Trimmed, single-spaced, lower-case name used for duplicate checks.</summary>
    public required string NormalizedName { get; init; }
    public string? Email { get; init; }
    public string? NormalizedEmail { get; init; }
    public required DateTime CreatedUtc { get; init; }
}
