using TabletopEvents.Api.Models.Dtos.Responses;

namespace TabletopEvents.Api.Models.Results;

/// <summary>Either the created event, or validation errors keyed by request field name.</summary>
public sealed record CreateEventResult(EventDetailsResponse? Event, IReadOnlyDictionary<string, string[]> Errors)
{
    public static CreateEventResult Success(EventDetailsResponse created) => new(created, new Dictionary<string, string[]>());

    public static CreateEventResult Invalid(IReadOnlyDictionary<string, string[]> errors) => new(null, errors);
}
