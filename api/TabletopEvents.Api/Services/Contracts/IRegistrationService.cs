using TabletopEvents.Api.Models.Dtos.Requests;
using TabletopEvents.Api.Models.Dtos.Responses;
using TabletopEvents.Api.Models.Results;

namespace TabletopEvents.Api.Services.Contracts;

public interface IRegistrationService
{
    RegistrationResult Register(int eventId, RegisterRequest request);

    IReadOnlyList<RegistrantResponse> List(int eventId);
}
