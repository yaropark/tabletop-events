namespace TabletopEvents.Api.Services.Contracts;

/// <summary>Builds absolute links (registration, event page) that players' phones can open.</summary>
public interface IPublicUrlService
{
    string BaseUrl { get; }

    string RegistrationUrl(int eventId);

    string EventUrl(int eventId);
}
