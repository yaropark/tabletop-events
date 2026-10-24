using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;

namespace TabletopEvents.Api.Events;

public static class CalendarInvite
{
    /// <summary>Builds a single-event iCalendar file. Times are written in UTC so every client agrees.</summary>
    public static string Create(EventDetails e, string eventPageUrl, DateTime utcNow)
    {
        var calendar = new Calendar { Method = "PUBLISH" };
        calendar.Events.Add(new CalendarEvent
        {
            Uid = $"event-{e.Id}@tabletop-events",
            Summary = e.Name,
            Description = $"{e.GameName} · {e.Format}\nRegister: {e.RegistrationUrl}",
            Location = e.Location,
            Start = new CalDateTime(e.StartUtc),
            End = new CalDateTime(e.EndUtc),
            DtStamp = new CalDateTime(utcNow),
            Url = new Uri(eventPageUrl),
        });
        return new CalendarSerializer().SerializeToString(calendar)!;
    }
}
