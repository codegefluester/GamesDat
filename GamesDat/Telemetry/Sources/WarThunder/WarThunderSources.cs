using System;

namespace GamesDat.Core.Telemetry.Sources.WarThunder;

/// <summary>
/// Factory methods for creating War Thunder telemetry sources with sensible defaults.
/// </summary>
public static class WarThunderSources
{
    private const string DefaultBaseUrl = "http://localhost:8111";

    /// <summary>
    /// Creates a StateSource for the /state endpoint with Hz-based configuration.
    /// </summary>
    /// <param name="baseUrl">Base URL of the War Thunder API. Defaults to http://localhost:8111.</param>
    /// <param name="hz">Polling frequency in Hz (polls per second). Default is 60Hz.</param>
    /// <returns>A configured StateSource instance.</returns>
    public static StateSource CreateStateSource(string? baseUrl = null, int hz = 60)
    {
        if (hz <= 0)
            throw new ArgumentOutOfRangeException(nameof(hz), hz, "Polling frequency must be greater than 0");
        
        var pollInterval = TimeSpan.FromMilliseconds(1000.0 / hz);
        return new StateSource(baseUrl ?? DefaultBaseUrl, pollInterval);
    }

    /// <summary>
    /// Creates a StateSource with custom options.
    /// </summary>
    /// <param name="options">Custom configuration options.</param>
    /// <returns>A configured StateSource instance.</returns>
    public static StateSource CreateStateSource(HttpPollingSourceOptions options)
    {
        return new StateSource(options);
    }

    /// <summary>
    /// Creates an IndicatorsSource for the /indicators endpoint with Hz-based configuration.
    /// </summary>
    /// <param name="baseUrl">Base URL of the War Thunder API. Defaults to http://localhost:8111.</param>
    /// <param name="hz">Polling frequency in Hz (polls per second). Default is 10Hz.</param>
    /// <returns>A configured IndicatorsSource instance.</returns>
    public static IndicatorsSource CreateIndicatorsSource(string? baseUrl = null, int hz = 10)
    {
        if (hz <= 0)
            throw new ArgumentOutOfRangeException(nameof(hz), hz, "Polling frequency must be greater than 0");
        
        var pollInterval = TimeSpan.FromMilliseconds(1000.0 / hz);
        return new IndicatorsSource(baseUrl ?? DefaultBaseUrl, pollInterval);
    }

    /// <summary>
    /// Creates an IndicatorsSource with custom options.
    /// </summary>
    /// <param name="options">Custom configuration options.</param>
    /// <returns>A configured IndicatorsSource instance.</returns>
    public static IndicatorsSource CreateIndicatorsSource(HttpPollingSourceOptions options)
    {
        return new IndicatorsSource(options);
    }
}
