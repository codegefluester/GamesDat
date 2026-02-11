using System;
using System.Net.Http;

namespace GamesDat.Core.Telemetry.Sources.WarThunder;

/// <summary>
/// Provides a shared HttpClient instance for all War Thunder telemetry sources.
/// Uses a single instance to prevent socket exhaustion.
/// Note: the shared client does not set a hard Timeout so per-request cancellation
/// (used by HttpPollingSourceBase) can control timeouts per-source.
/// </summary>
internal static class WarThunderHttpClient
{
    private static readonly Lazy<HttpClient> _lazyClient = new(() =>
    {
        var client = new HttpClient
        {
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };
        return client;
    });

    /// <summary>
    /// Gets the shared HttpClient instance.
    /// </summary>
    public static HttpClient Instance => _lazyClient.Value;
}
