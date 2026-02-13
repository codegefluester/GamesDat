namespace GamesDat.Core.Telemetry.Sources.WarThunder;

/// <summary>
/// Configuration options for War Thunder StateSource.
/// </summary>
public class StateSourceOptions
{
    /// <summary>
    /// HTTP polling options (URL, intervals, retry settings).
    /// </summary>
    public required HttpPollingSourceOptions HttpOptions { get; init; }
}
