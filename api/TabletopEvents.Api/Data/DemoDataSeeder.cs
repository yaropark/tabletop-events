using TabletopEvents.Api.Models.Dtos.Requests;
using TabletopEvents.Api.Repositories.Contracts;
using TabletopEvents.Api.Services.Contracts;

namespace TabletopEvents.Api.Data;

/// <summary>Seeds a few upcoming events on an empty database so the calendar has something to show.</summary>
public sealed class DemoDataSeeder(
    IEventRepository eventRepository,
    IEventService events,
    IRegistrationService registrations,
    IStoreClock clock)
{
    private static readonly string[] Players =
    [
        "Ava Martin", "Ben Okafor", "Chloe Nguyen", "Diego Ramos", "Emma Schultz", "Finn O'Brien",
        "Grace Liu", "Hiro Tanaka", "Isla Novak", "Jonah Weiss", "Kara Singh", "Leo Rossi",
    ];

    public void SeedIfEmpty()
    {
        if (eventRepository.Any())
            return;

        var today = clock.LocalNow.Date;
        DateTime On(DayOfWeek day, int hour, int extraWeeks = 0)
        {
            var days = ((int)day - (int)today.DayOfWeek + 7) % 7;
            return today.AddDays(days == 0 ? 7 : days).AddDays(7 * extraWeeks).AddHours(hour);
        }

        (CreateEventRequest Request, int Registrations)[] seed =
        [
            (Event("Friday Night Magic", "mtg", On(DayOfWeek.Friday, 18), "Commander", capacity: 16), 5),
            (Event("Pokémon League Challenge", "pokemon", On(DayOfWeek.Saturday, 11), "Standard"), 9),
            (Event("Star Wars: Unlimited Store Showdown", "swu", On(DayOfWeek.Sunday, 13), "Premier", capacity: 8), 7),
            (Event("Booster Draft Night", "mtg", On(DayOfWeek.Wednesday, 18), "Draft", capacity: 8, duration: 180), 8),
            (Event("Twin Suns Casual", "swu", On(DayOfWeek.Thursday, 18, extraWeeks: 1), "Twin Suns", capacity: 12), 2),
        ];

        foreach (var (request, count) in seed)
        {
            if (events.Create(request).Event is not { } created)
                continue;
            foreach (var player in Players.Take(count))
                registrations.Register(created.Id, new RegisterRequest { Name = player });
        }
    }

    private static CreateEventRequest Event(
        string name, string templateKey, DateTime startLocal, string format, int? capacity = null, int? duration = null) =>
        new()
        {
            Name = name,
            TemplateKey = templateKey,
            StartLocal = startLocal,
            Format = format,
            Capacity = capacity,
            DurationMinutes = duration,
        };
}
