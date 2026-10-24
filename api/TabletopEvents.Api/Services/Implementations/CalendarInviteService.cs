using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Models.Results;
using TabletopEvents.Api.Services.Contracts;

namespace TabletopEvents.Api.Services.Implementations;

public sealed partial class CalendarInviteService(IPublicUrlService urls, IStoreClock clock) : ICalendarInviteService
{
    /// <summary>Builds a single-event iCalendar file. Times are written in UTC so every client agrees.</summary>
    public CalendarFile Create(EventDetailsResponse e)
    {
        var calendar = new Ical.Net.Calendar { Method = "PUBLISH" };
        calendar.Events.Add(new CalendarEvent
        {
            Uid = $"event-{e.Id}@tabletop-events",
            Summary = e.Name,
            Description = $"{e.GameName} · {e.Format}\nRegister: {e.RegistrationUrl}",
            Location = e.Location,
            Start = new CalDateTime(e.StartUtc),
            End = new CalDateTime(e.EndUtc),
            DtStamp = new CalDateTime(clock.UtcNow),
            Url = new Uri(urls.EventUrl(e.Id)),
        });

        var ics = new CalendarSerializer().SerializeToString(calendar)!;
        return new CalendarFile(Encoding.UTF8.GetBytes(ics), $"{Slug(e.Name)}.ics");
    }

    private static string Slug(string name)
    {
        // "Pokémon" -> "pokemon": split accents off their letters and drop them before slugging.
        var plain = string.Concat(name.Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));
        var slug = NonSlugChars().Replace(plain.ToLowerInvariant(), "-").Trim('-');
        return slug.Length == 0 ? "event" : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugChars();
}
