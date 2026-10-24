using TabletopEvents.Api.Models.Dtos.Requests;
using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Models.Results;

namespace TabletopEvents.Api.Services.Contracts;

public interface IEventService
{
    CreateEventResult Create(CreateEventRequest request);

    EventDetailsResponse? Get(int id);

    /// <summary>Events starting between two store-local times; null bounds are open.</summary>
    IReadOnlyList<EventSummaryResponse> List(DateTime? fromLocal, DateTime? toLocal);
}
