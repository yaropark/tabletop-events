using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Models.Enums;

namespace TabletopEvents.Api.Models.Results;

public sealed record RegistrationResult(RegistrationOutcome Outcome, RegistrationResponse? Registration = null);
