using TabletopEvents.Api.Models.Entities;

namespace TabletopEvents.Api.Repositories.Contracts;

public interface IEventRepository
{
    /// <summary>Inserts the event and returns its new id.</summary>
    int Insert(Event evt);

    Event? Get(int id);

    EventOverview? GetOverview(int id);

    /// <summary>Events starting in [fromUtc, toUtc), ordered by start time. Null bounds are open.</summary>
    IReadOnlyList<EventOverview> ListOverviews(DateTime? fromUtc, DateTime? toUtc);

    bool Any();
}
