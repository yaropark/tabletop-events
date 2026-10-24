using Microsoft.Extensions.Options;

namespace TabletopEvents.Api;

public sealed class StoreOptions
{
    public string Name { get; set; } = "Corner Table Games";
    public string Address { get; set; } = "";
    /// <summary>IANA time zone id. Organizers enter event times as wall-clock time in this zone.</summary>
    public string TimeZone { get; set; } = "America/Chicago";
    /// <summary>Base URL players reach the app on. Used for registration links and QR codes.</summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:8080";
}

public sealed class StoreClock(IOptions<StoreOptions> options, TimeProvider time)
{
    public TimeZoneInfo TimeZone { get; } = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);

    public DateTime UtcNow => time.GetUtcNow().UtcDateTime;

    public DateTime LocalNow => ToLocal(UtcNow);

    public DateTime ToUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), TimeZone);

    public DateTime ToLocal(DateTime utc) =>
        DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZone), DateTimeKind.Unspecified);
}
