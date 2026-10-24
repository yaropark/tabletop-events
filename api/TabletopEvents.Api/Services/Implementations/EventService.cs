using Microsoft.Extensions.Options;
using TabletopEvents.Api.Models.Dtos.Requests;
using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Models.Entities;
using TabletopEvents.Api.Models.Results;
using TabletopEvents.Api.Options;
using TabletopEvents.Api.Repositories.Contracts;
using TabletopEvents.Api.Services.Contracts;

namespace TabletopEvents.Api.Services.Implementations;

public sealed class EventService(
    IEventRepository events,
    ITemplateRepository templates,
    IStoreClock clock,
    IPublicUrlService urls,
    IOptions<StoreOptions> store) : IEventService
{
    public CreateEventResult Create(CreateEventRequest request)
    {
        // Required fields and plain ranges are already checked by the request's attributes;
        // these are the rules that depend on the chosen game or on the current time.
        var errors = new Dictionary<string, string[]>();
        void Fail(string field, string message) => errors[field] = [message];

        var template = string.IsNullOrWhiteSpace(request.TemplateKey) ? null : templates.Get(request.TemplateKey);
        if (template is null)
        {
            Fail("templateKey", "Choose a valid game.");
            return CreateEventResult.Invalid(errors);
        }

        // Every game-specific value comes from the template; nothing here knows which game it is.
        var format = string.IsNullOrWhiteSpace(request.Format) ? template.Formats[0] : request.Format.Trim();
        if (!template.Formats.Contains(format))
            Fail("format", $"{template.Name} supports: {string.Join(", ", template.Formats)}.");

        var capacity = request.Capacity ?? template.DefaultCapacity;
        if (capacity < template.MinPlayers || capacity > template.MaxCapacity)
            Fail("capacity", $"Capacity for {template.Name} must be between {template.MinPlayers} and {template.MaxCapacity}.");

        DateTime startUtc = default;
        if (request.StartLocal is not { } startLocal)
            Fail("startLocal", "Date and start time are required.");
        else if (clock.TimeZone.IsInvalidTime(DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified)))
            Fail("startLocal", "That time does not exist in the store's time zone (daylight saving change).");
        else if ((startUtc = clock.ToUtc(startLocal)) <= clock.UtcNow)
            Fail("startLocal", "Start time must be in the future.");

        if (errors.Count > 0)
            return CreateEventResult.Invalid(errors);

        var id = events.Insert(new Event
        {
            TemplateKey = template.Key,
            Name = request.Name!.Trim(),
            Format = format,
            StartUtc = startUtc,
            DurationMinutes = request.DurationMinutes ?? template.DefaultDurationMinutes,
            Capacity = capacity,
            MinPlayers = template.MinPlayers,
            Location = string.IsNullOrWhiteSpace(request.Location) ? store.Value.Address : request.Location.Trim(),
            CreatedUtc = clock.UtcNow,
        });

        return CreateEventResult.Success(Get(id)!);
    }

    public EventDetailsResponse? Get(int id) =>
        events.GetOverview(id) is { } overview ? ToDetails(overview) : null;

    public IReadOnlyList<EventSummaryResponse> List(DateTime? fromLocal, DateTime? toLocal) =>
        events.ListOverviews(
                fromLocal is { } from ? clock.ToUtc(from) : null,
                toLocal is { } to ? clock.ToUtc(to) : null)
            .Select(o => new EventSummaryResponse
            {
                Id = o.Event.Id,
                Name = o.Event.Name,
                TemplateKey = o.Event.TemplateKey,
                GameName = o.GameName,
                Color = o.GameColor,
                Format = o.Event.Format,
                StartLocal = clock.ToLocal(o.Event.StartUtc),
                EndLocal = clock.ToLocal(o.Event.EndUtc),
                Capacity = o.Event.Capacity,
                RegisteredCount = o.RegisteredCount,
            })
            .ToList();

    private EventDetailsResponse ToDetails(EventOverview o)
    {
        var e = o.Event;
        var seatsLeft = Math.Max(0, e.Capacity - o.RegisteredCount);
        return new EventDetailsResponse
        {
            Id = e.Id,
            Name = e.Name,
            TemplateKey = e.TemplateKey,
            GameName = o.GameName,
            Color = o.GameColor,
            Format = e.Format,
            StartLocal = clock.ToLocal(e.StartUtc),
            EndLocal = clock.ToLocal(e.EndUtc),
            StartUtc = e.StartUtc,
            EndUtc = e.EndUtc,
            TimeZone = store.Value.TimeZone,
            DurationMinutes = e.DurationMinutes,
            Location = e.Location,
            Capacity = e.Capacity,
            MinPlayers = e.MinPlayers,
            RegisteredCount = o.RegisteredCount,
            SeatsLeft = seatsLeft,
            RegistrationOpen = seatsLeft > 0 && e.StartUtc > clock.UtcNow,
            RegistrationUrl = urls.RegistrationUrl(e.Id),
        };
    }
}
