namespace TabletopEvents.Api.Services.Contracts;

/// <summary>Current time and conversions between UTC and the store's wall-clock time.</summary>
public interface IStoreClock
{
    TimeZoneInfo TimeZone { get; }

    DateTime UtcNow { get; }

    DateTime LocalNow { get; }

    DateTime ToUtc(DateTime local);

    DateTime ToLocal(DateTime utc);
}
