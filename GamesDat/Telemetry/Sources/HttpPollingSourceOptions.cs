using System;
using System.Collections.Generic;
using System.Linq;

namespace GamesDat.Core.Telemetry.Sources;

/// <summary>
/// Configuration options for HTTP polling-based telemetry sources.
/// </summary>
public class HttpPollingSourceOptions
{
    /// <summary>
    /// Base URL of the HTTP endpoint (e.g., "http://localhost:8111").
    /// </summary>
    public required string BaseUrl { get; init; }

    /// <summary>
    /// Relative path to the endpoint (e.g., "/state").
    /// </summary>
    public required string EndpointPath { get; init; }

    /// <summary>
    /// Time interval between polls. Default is 100ms.
    /// </summary>
    public TimeSpan PollInterval { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// HTTP request timeout. Default is 5 seconds.
    /// </summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum number of consecutive errors before giving up. Default is 10.
    /// </summary>
    public int MaxConsecutiveErrors { get; init; } = 10;

    /// <summary>
    /// Initial delay before first retry after an error. Default is 1 second.
    /// </summary>
    public TimeSpan InitialRetryDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum delay between retries (exponential backoff cap). Default is 30 seconds.
    /// </summary>
    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Optional custom HTTP headers to include with requests.
    /// </summary>
    public Dictionary<string, string>? Headers { get; init; }

    /// <summary>
    /// Optional query string parameters to append to the endpoint URL.
    /// </summary>
    public Dictionary<string, string>? QueryParameters { get; init; }

    /// <summary>
    /// Gets the full URL by combining BaseUrl and EndpointPath.
    /// </summary>
    public string GetFullUrl()
    {
        var baseUrl = BaseUrl.TrimEnd('/');
        var endpointPath = EndpointPath.StartsWith('/') ? EndpointPath : $"/{EndpointPath}";
        var url = $"{baseUrl}{endpointPath}";

        if (QueryParameters == null || QueryParameters.Count == 0)
            return url;

        var queryString = string.Join("&", QueryParameters.Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        return $"{url}?{queryString}";
    }
}