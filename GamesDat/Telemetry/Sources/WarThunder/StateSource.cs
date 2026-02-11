using System;

namespace GamesDat.Core.Telemetry.Sources.WarThunder;

/// <summary>
/// Telemetry source for War Thunder's /state endpoint.
/// Provides primary flight/vehicle telemetry data at high frequency (recommended 60Hz).
/// </summary>
public class StateSource : HttpPollingSourceBase<StateData>
{
    /// <summary>
    /// Initializes a new instance with custom options.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    public StateSource(HttpPollingSourceOptions options)
        : base(WarThunderHttpClient.Instance, options, ownsClient: false)
    {
    }

    /// <summary>
    /// Initializes a new instance with simplified parameters.
    /// </summary>
    /// <param name="baseUrl">Base URL of the War Thunder API.</param>
    /// <param name="pollInterval">Time between polls.</param>
    public StateSource(string baseUrl, TimeSpan pollInterval)
        : this(new HttpPollingSourceOptions
        {
            BaseUrl = baseUrl,
            EndpointPath = "/state",
            PollInterval = pollInterval
        })
    {
    }
}
