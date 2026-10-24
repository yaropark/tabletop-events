using TabletopEvents.Api.Models.Entities;
using TabletopEvents.Api.Models.Enums;

namespace TabletopEvents.Api.Repositories.Contracts;

public interface IRegistrationRepository
{
    /// <summary>
    /// Inserts the registration only if the event exists, has not started, and has a free seat.
    /// The check and the insert are atomic, so concurrent calls can never overbook.
    /// </summary>
    (RegistrationInsertStatus Status, int Id) TryInsertWithinCapacity(Registration registration, DateTime nowUtc);

    /// <summary>True if a registration with the same identity (email if given, else name) exists.</summary>
    bool Exists(int eventId, string normalizedName, string? normalizedEmail);

    int Count(int eventId);

    IReadOnlyList<Registration> ListByEvent(int eventId);
}
