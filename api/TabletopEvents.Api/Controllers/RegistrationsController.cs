using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TabletopEvents.Api.Models.Dtos.Requests;
using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Models.Enums;
using TabletopEvents.Api.Services.Contracts;

namespace TabletopEvents.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/registrations")]
public sealed class RegistrationsController(IRegistrationService registrations) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<RegistrantResponse>> List(int eventId) => Ok(registrations.List(eventId));

    /// <summary>Registers a player. Capacity and duplicates are enforced here on the server, not in the UI.</summary>
    [HttpPost]
    public ActionResult<RegistrationResponse> Register(int eventId, RegisterRequest request)
    {
        var result = registrations.Register(eventId, request);
        return result.Outcome switch
        {
            RegistrationOutcome.Registered => Created($"/api/events/{eventId}/registrations", result.Registration),
            RegistrationOutcome.EventNotFound => Problem("Event not found.", statusCode: 404),
            RegistrationOutcome.EventFull => Problem("Sorry, this event is full.", statusCode: 409, title: "Event full"),
            RegistrationOutcome.RegistrationClosed => Problem("Registration closed when the event started.", statusCode: 409, title: "Registration closed"),
            RegistrationOutcome.AlreadyRegistered => Problem("You're already registered for this event.", statusCode: 409, title: "Already registered"),
            _ => throw new UnreachableException(),
        };
    }
}
