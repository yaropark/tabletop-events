using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using QRCoder;
using TabletopEvents.Api.Events;
using TabletopEvents.Api.Registrations;
using TabletopEvents.Api.Templates;

namespace TabletopEvents.Api;

public static partial class Endpoints
{
    public static void MapApi(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/templates", (TemplateRepository templates) => templates.GetAll());

        api.MapGet("/events", (DateTime? from, DateTime? to, EventService events) => events.List(from, to));

        api.MapPost("/events", (CreateEventRequest request, EventService events) =>
        {
            var (created, errors) = events.Create(request);
            return created is null
                ? Results.ValidationProblem(errors)
                : Results.Created($"/api/events/{created.Id}", created);
        });

        api.MapGet("/events/{id:int}", (int id, EventService events) =>
            events.Get(id) is { } e ? Results.Ok(e) : Results.NotFound());

        api.MapGet("/events/{id:int}/qr.png", (int id, EventService events) =>
            events.Get(id) is { } e
                ? Results.File(PngByteQRCodeHelper.GetQRCode(e.RegistrationUrl, QRCodeGenerator.ECCLevel.Q, 10), "image/png")
                : Results.NotFound());

        api.MapGet("/events/{id:int}/invite.ics", (int id, EventService events, StoreClock clock, IOptions<StoreOptions> store) =>
        {
            if (events.Get(id) is not { } e)
                return Results.NotFound();

            var eventPageUrl = $"{store.Value.PublicBaseUrl.TrimEnd('/')}/events/{e.Id}";
            var ics = CalendarInvite.Create(e, eventPageUrl, clock.UtcNow);
            return Results.File(Encoding.UTF8.GetBytes(ics), "text/calendar; charset=utf-8", $"{Slug(e.Name)}.ics");
        });

        api.MapGet("/events/{id:int}/registrations", (int id, RegistrationService registrations) =>
            registrations.List(id));

        api.MapPost("/events/{id:int}/registrations", (int id, RegisterRequest request, RegistrationService registrations) =>
        {
            var result = registrations.Register(id, request);
            return result.Outcome switch
            {
                RegistrationOutcome.Registered => Results.Created($"/api/events/{id}/registrations", result.Registration),
                RegistrationOutcome.Invalid => Results.ValidationProblem(result.Errors!),
                RegistrationOutcome.EventNotFound => Results.Problem("Event not found.", statusCode: 404),
                RegistrationOutcome.EventFull => Results.Problem("Sorry, this event is full.", statusCode: 409, title: "Event full"),
                RegistrationOutcome.RegistrationClosed => Results.Problem("Registration closed when the event started.", statusCode: 409, title: "Registration closed"),
                RegistrationOutcome.AlreadyRegistered => Results.Problem("You're already registered for this event.", statusCode: 409, title: "Already registered"),
                _ => throw new UnreachableException(),
            };
        });

        // Unknown /api routes stay 404 instead of falling through to the Angular app.
        api.Map("/{**path}", () => Results.NotFound());
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
