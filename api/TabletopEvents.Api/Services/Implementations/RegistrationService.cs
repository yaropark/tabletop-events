using TabletopEvents.Api.Models.Dtos.Requests;
using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Models.Entities;
using TabletopEvents.Api.Models.Enums;
using TabletopEvents.Api.Models.Results;
using TabletopEvents.Api.Repositories.Contracts;
using TabletopEvents.Api.Services.Contracts;

namespace TabletopEvents.Api.Services.Implementations;

public sealed class RegistrationService(
    IRegistrationRepository registrations,
    IEventRepository events,
    IStoreClock clock) : IRegistrationService
{
    public RegistrationResult Register(int eventId, RegisterRequest request)
    {
        // Name and email format are already validated by the request's attributes.
        var name = string.Join(' ', (request.Name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        var normalizedName = name.ToLowerInvariant();
        var normalizedEmail = email?.ToLowerInvariant();
        var now = clock.UtcNow;

        var (status, id) = registrations.TryInsertWithinCapacity(new Registration
        {
            EventId = eventId,
            Name = name,
            NormalizedName = normalizedName,
            Email = email,
            NormalizedEmail = normalizedEmail,
            CreatedUtc = now,
        }, now);

        if (status == RegistrationInsertStatus.Duplicate)
            return new(RegistrationOutcome.AlreadyRegistered);

        var evt = events.Get(eventId);
        if (status == RegistrationInsertStatus.Inserted)
        {
            var seatsLeft = Math.Max(0, evt!.Capacity - registrations.Count(eventId));
            return new(RegistrationOutcome.Registered, new RegistrationResponse { Id = id, Name = name, SeatsLeft = seatsLeft });
        }

        // Nothing was inserted. The decision is already made; these reads only pick the message.
        if (evt is null)
            return new(RegistrationOutcome.EventNotFound);
        if (registrations.Exists(eventId, normalizedName, normalizedEmail))
            return new(RegistrationOutcome.AlreadyRegistered);
        return evt.StartUtc <= now
            ? new(RegistrationOutcome.RegistrationClosed)
            : new(RegistrationOutcome.EventFull);
    }

    public IReadOnlyList<RegistrantResponse> List(int eventId) =>
        registrations.ListByEvent(eventId)
            .Select(r => new RegistrantResponse { Name = r.Name, RegisteredUtc = r.CreatedUtc })
            .ToList();
}
