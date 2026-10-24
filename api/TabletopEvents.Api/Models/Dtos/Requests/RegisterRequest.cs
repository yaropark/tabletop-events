using System.ComponentModel.DataAnnotations;

namespace TabletopEvents.Api.Models.Dtos.Requests;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Please enter your name (max 100 characters).")]
    [StringLength(100, ErrorMessage = "Please enter your name (max 100 characters).")]
    public string? Name { get; set; }

    /// <summary>Optional. When given, it identifies the player for duplicate checks instead of the name.</summary>
    [EmailAddress(ErrorMessage = "That email address doesn't look right.")]
    [StringLength(254, ErrorMessage = "That email address doesn't look right.")]
    public string? Email { get; set; }
}
