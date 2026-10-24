using System.Globalization;

namespace TabletopEvents.Api.Data;

/// <summary>
/// Dates are stored as fixed-width ISO 8601 UTC text ("O" format), so string comparison in SQL
/// sorts and filters them correctly.
/// </summary>
public static class SqliteDates
{
    public static string Format(DateTime utc) => utc.ToUniversalTime().ToString("O");

    public static DateTime Parse(string value) => DateTime.Parse(value, null, DateTimeStyles.RoundtripKind);
}
