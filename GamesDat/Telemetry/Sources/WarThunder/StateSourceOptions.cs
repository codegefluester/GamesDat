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

    /// <summary>
    /// If true, frames where Valid=false will be skipped (not yielded).
    /// When enabled, the source will continue polling but only yield valid frames.
    /// Default is true (skip invalid frames).
    /// </summary>
    public bool SkipInvalidFrames { get; init; } = true;

    /// <summary>
    /// Interval for logging "waiting for valid data" messages when skipping invalid frames.
    /// Set to TimeSpan.Zero to disable these messages. Default is 10 seconds.
    /// </summary>
    public TimeSpan InvalidFrameLogInterval { get; init; } = TimeSpan.FromSeconds(10);
}
