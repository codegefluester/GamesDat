using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12023
{
    /// <summary>
    /// Lap data packet for F1 2023. Details of all cars in the session.
    /// Frequency: Rate as specified in menus
    /// Size: 1131 bytes
    /// Version: 1
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketLapData
    {
        public PacketHeader m_header;              // Header

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public LapData[] m_lapData;                // Lap data for all cars on track

        public byte m_timeTrialPBCarIdx;           // Index of Personal Best car in time trial (255 if invalid)
        public byte m_timeTrialRivalCarIdx;        // Index of Rival car in time trial (255 if invalid)
    }
}
