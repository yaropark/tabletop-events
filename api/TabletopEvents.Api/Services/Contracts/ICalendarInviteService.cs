using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Models.Results;

namespace TabletopEvents.Api.Services.Contracts;

public interface ICalendarInviteService
{
    /// <summary>Builds a downloadable .ics file for the event.</summary>
    CalendarFile Create(EventDetailsResponse evt);
}
