using System;

namespace GamesDat.Core.Telemetry.Sources.Trackmania
{
    /// <summary>
    /// Pre-configured sources for Trackmania telemetry via ManiaPlanet_Telemetry shared memory
    /// </summary>
    public static class TrackmaniaMemoryMappedSource
    {
        /// <summary>
        /// Name of the shared memory mapped file used by Trackmania for telemetry
        /// Available in Maniaplanet 4, Trackmania (2020), and Trackmania Turbo
        /// </summary>
        public const string TelemetryMapName = "ManiaPlanet_Telemetry";

        /// <summary>
        /// Creates a telemetry source for Trackmania that reads from the ManiaPlanet_Telemetry shared memory
        /// </summary>
        /// <param name="pollInterval">How often to poll the shared memory for updates. Default is 16ms (~60Hz)</param>
        /// <returns>A configured TrackmaniaMemoryMappedFileSource for Trackmania telemetry</returns>
        public static TrackmaniaMemoryMappedFileSource CreateTelemetrySource(TimeSpan? pollInterval = null)
        {
            return new TrackmaniaMemoryMappedFileSource(
                TelemetryMapName,
                pollInterval ?? TimeSpan.FromMilliseconds(16) // ~60Hz default
            );
        }
    }
}
