namespace TabletopEvents.Api.Options;

public sealed class StoreOptions
{
    public string Name { get; set; } = "Corner Table Games";
    public string Address { get; set; } = "";
    /// <summary>IANA time zone id. Organizers enter event times as wall-clock time in this zone.</summary>
    public string TimeZone { get; set; } = "America/Chicago";
    /// <summary>
    /// Base URL players reach the app on, for registration links and QR codes.
    /// Optional: when empty, see PublicUrlService for how it is discovered.
    /// </summary>
    public string PublicBaseUrl { get; set; } = "";
    /// <summary>Optional cloudflared metrics address; when the tunnel runs, its public hostname is used.</summary>
    public string TunnelMetricsUrl { get; set; } = "";
}
