using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Trackmania
{
    /// <summary>
    /// Header structure that precedes the telemetry data in ManiaPlanet_Telemetry shared memory
    /// Total size: 44 bytes
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct TrackmaniaMemoryHeader
    {
        /// <summary>
        /// Magic string "ManiaPlanet_Telemetry" (22 bytes)
        /// </summary>
        public fixed byte Magic[22];

        /// <summary>
        /// Padding bytes (10 bytes)
        /// </summary>
        public fixed byte Padding[10];

        /// <summary>
        /// Version of the telemetry structure (expected: 3)
        /// </summary>
        public uint Version;

        /// <summary>
        /// Total size of the data structure in bytes
        /// </summary>
        public uint Size;

        /// <summary>
        /// Synchronization counter - increments with each update
        /// Even = data is stable, Odd = data is being written
        /// </summary>
        public uint UpdateNumber;
    }
}
