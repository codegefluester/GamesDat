using System;

namespace GamesDat.Core.Telemetry.Sources.WarThunder;

/// <summary>
/// Telemetry source for War Thunder's /indicators endpoint.
/// Provides cockpit instrumentation data at lower frequency (recommended 10Hz).
/// </summary>
public class IndicatorsSource : HttpPollingSourceBase<IndicatorsData>
{
    /// <summary>
    /// Initializes a new instance with custom options.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    public IndicatorsSource(HttpPollingSourceOptions options)
        : base(WarThunderHttpClient.Instance, options, ownsClient: false)
    {
    }

    /// <summary>
    /// Initializes a new instance with simplified parameters.
    /// </summary>
    /// <param name="baseUrl">Base URL of the War Thunder API.</param>
    /// <param name="pollInterval">Time between polls.</param>
    public IndicatorsSource(string baseUrl, TimeSpan pollInterval)
        : this(new HttpPollingSourceOptions
        {
            BaseUrl = baseUrl,
            EndpointPath = "/indicators",
            PollInterval = pollInterval
        })
    {
    }
}
