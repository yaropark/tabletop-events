using Dapper;
using TabletopEvents.Api.Events;
using TabletopEvents.Api.Registrations;

namespace TabletopEvents.Api.Data;

/// <summary>Seeds a few upcoming events on an empty database so the calendar has something to show.</summary>
public static class DemoData
{
    private static readonly string[] Players =
    [
        "Ava Martin", "Ben Okafor", "Chloe Nguyen", "Diego Ramos", "Emma Schultz", "Finn O'Brien",
        "Grace Liu", "Hiro Tanaka", "Isla Novak", "Jonah Weiss", "Kara Singh", "Leo Rossi",
    ];

    public static void SeedIfEmpty(Database db, EventService events, RegistrationService registrations, StoreClock clock)
    {
        using (var connection = db.Open())
        {
            if (connection.ExecuteScalar<long>("SELECT COUNT(*) FROM Events") > 0)
                return;
        }

        var today = clock.LocalNow.Date;
        DateTime On(DayOfWeek day, int hour, int extraWeeks = 0)
        {
            var days = ((int)day - (int)today.DayOfWeek + 7) % 7;
            return today.AddDays(days == 0 ? 7 : days).AddDays(7 * extraWeeks).AddHours(hour);
        }

        (CreateEventRequest Request, int Registrations)[] seed =
        [
            (new("Friday Night Magic", "mtg", On(DayOfWeek.Friday, 18), "Commander", null, 16, null), 5),
            (new("Pokémon League Challenge", "pokemon", On(DayOfWeek.Saturday, 11), "Standard", null, null, null), 9),
            (new("Star Wars: Unlimited Store Showdown", "swu", On(DayOfWeek.Sunday, 13), "Premier", null, 8, null), 7),
            (new("Booster Draft Night", "mtg", On(DayOfWeek.Wednesday, 18), "Draft", 180, 8, null), 8),
            (new("Twin Suns Casual", "swu", On(DayOfWeek.Thursday, 18, extraWeeks: 1), "Twin Suns", null, 12, null), 2),
        ];

        foreach (var (request, count) in seed)
        {
            var (created, _) = events.Create(request);
            if (created is null)
                continue;
            foreach (var player in Players.Take(count))
                registrations.Register(created.Id, new RegisterRequest(player, null));
        }
    }
}
