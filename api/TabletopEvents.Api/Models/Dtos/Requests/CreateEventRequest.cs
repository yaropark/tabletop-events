using System.ComponentModel.DataAnnotations;
using TabletopEvents.Api.Models.Entities;

namespace TabletopEvents.Api.Models.Dtos.Requests;

/// <summary>
/// Field-level rules live here and are checked automatically by [ApiController].
/// Rules that depend on the chosen game (format, capacity range) are checked in EventService.
/// </summary>
public sealed class CreateEventRequest
{
    [Required(ErrorMessage = "Name is required (max 120 characters).")]
    [StringLength(120, ErrorMessage = "Name is required (max 120 characters).")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Choose a valid game.")]
    public string? TemplateKey { get; set; }

    /// <summary>Wall-clock start time in the store's time zone, e.g. 2026-10-30T18:00.</summary>
    [Required(ErrorMessage = "Date and start time are required.")]
    public DateTime? StartLocal { get; set; }

    /// <summary>Optional; defaults to the template's first format.</summary>
    public string? Format { get; set; }

    /// <summary>Optional; defaults to the template's duration.</summary>
    [Range(15, 24 * 60, ErrorMessage = "Duration must be between 15 minutes and 24 hours.")]
    public int? DurationMinutes { get; set; }

    /// <summary>Optional; defaults to the template's default capacity.</summary>
    [Range(1, Event.MaxCapacity, ErrorMessage = "Capacity must be between 1 and 30.")]
    public int? Capacity { get; set; }

    /// <summary>Optional; defaults to the store address.</summary>
    [StringLength(200)]
    public string? Location { get; set; }
}
