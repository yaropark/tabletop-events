using Microsoft.AspNetCore.Mvc;
using TabletopEvents.Api.Models.Dtos.Requests;
using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Services.Contracts;

namespace TabletopEvents.Api.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController(
    IEventService events,
    IQrCodeService qrCodes,
    ICalendarInviteService invites) : ControllerBase
{
    /// <summary>Events starting in a store-local date range, for the calendar.</summary>
    [HttpGet]
    public ActionResult<IReadOnlyList<EventSummaryResponse>> List([FromQuery] DateTime? from, [FromQuery] DateTime? to) =>
        Ok(events.List(from, to));

    [HttpPost]
    public ActionResult<EventDetailsResponse> Create(CreateEventRequest request)
    {
        var result = events.Create(request);
        if (result.Event is null)
        {
            foreach (var (field, messages) in result.Errors)
                foreach (var message in messages)
                    ModelState.AddModelError(field, message);
            return ValidationProblem(ModelState);
        }

        return CreatedAtAction(nameof(Get), new { id = result.Event.Id }, result.Event);
    }

    [HttpGet("{id:int}")]
    public ActionResult<EventDetailsResponse> Get(int id) =>
        events.Get(id) is { } evt ? Ok(evt) : NotFound();

    /// <summary>QR code PNG encoding the event's registration link.</summary>
    [HttpGet("{id:int}/qr.png")]
    public IActionResult QrCode(int id) =>
        events.Get(id) is { } evt ? File(qrCodes.CreatePng(evt.RegistrationUrl), "image/png") : NotFound();

    /// <summary>Downloadable calendar invite (.ics) for Google Calendar, Outlook, etc.</summary>
    [HttpGet("{id:int}/invite.ics")]
    public IActionResult CalendarInvite(int id)
    {
        if (events.Get(id) is not { } evt)
            return NotFound();

        var file = invites.Create(evt);
        return File(file.Content, "text/calendar; charset=utf-8", file.FileName);
    }
}
