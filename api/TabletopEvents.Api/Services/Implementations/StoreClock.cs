using Microsoft.Extensions.Options;
using TabletopEvents.Api.Options;
using TabletopEvents.Api.Services.Contracts;

namespace TabletopEvents.Api.Services.Implementations;

public sealed class StoreClock(IOptions<StoreOptions> options, TimeProvider time) : IStoreClock
{
    public TimeZoneInfo TimeZone { get; } = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);

    public DateTime UtcNow => time.GetUtcNow().UtcDateTime;

    public DateTime LocalNow => ToLocal(UtcNow);

    public DateTime ToUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), TimeZone);

    public DateTime ToLocal(DateTime utc) =>
        DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(utc, TimeZone), DateTimeKind.Unspecified);
}
