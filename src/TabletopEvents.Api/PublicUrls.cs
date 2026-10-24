using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TabletopEvents.Api;

/// <summary>
/// Picks the base URL that goes into registration links and QR codes, so a phone can open it:
/// 1. Store:PublicBaseUrl when configured explicitly;
/// 2. the public https hostname of the optional cloudflared quick tunnel, when it is running;
/// 3. otherwise the address the current request came in on (e.g. http://192.168.1.20:8080).
/// </summary>
public sealed class PublicUrls(IOptions<StoreOptions> options, IHttpContextAccessor http, TimeProvider time, ILogger<PublicUrls> logger)
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(1) };
    private (string? Url, DateTimeOffset Expires) _tunnel;

    public string BaseUrl
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(options.Value.PublicBaseUrl))
                return options.Value.PublicBaseUrl.TrimEnd('/');

            if (TunnelUrl() is { } tunnel)
                return tunnel;

            return http.HttpContext?.Request is { } r
                ? $"{r.Scheme}://{r.Host}{r.PathBase}"
                : "http://localhost:8080";
        }
    }

    public string RegistrationUrl(int eventId) => $"{BaseUrl}/register/{eventId}";

    public string EventUrl(int eventId) => $"{BaseUrl}/events/{eventId}";

    /// <summary>Asks cloudflared's metrics endpoint for the quick-tunnel hostname; cached briefly either way.</summary>
    private string? TunnelUrl()
    {
        var metricsUrl = options.Value.TunnelMetricsUrl;
        if (string.IsNullOrWhiteSpace(metricsUrl))
            return null;

        var now = time.GetUtcNow();
        var cached = _tunnel;
        if (now < cached.Expires)
            return cached.Url;

        string? url = null;
        try
        {
            using var response = Client.Send(new HttpRequestMessage(HttpMethod.Get, $"{metricsUrl.TrimEnd('/')}/quicktunnel"));
            using var json = JsonDocument.Parse(response.Content.ReadAsStream());
            if (json.RootElement.TryGetProperty("hostname", out var host) && host.GetString() is { Length: > 0 } name)
                url = $"https://{name}";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogDebug("No cloudflared tunnel at {MetricsUrl}: {Message}", metricsUrl, ex.Message);
        }

        _tunnel = (url, now + (url is null ? TimeSpan.FromSeconds(30) : TimeSpan.FromMinutes(1)));
        return url;
    }
}
