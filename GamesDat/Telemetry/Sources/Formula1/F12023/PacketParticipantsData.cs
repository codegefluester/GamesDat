using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12023
{
    /// <summary>
    /// Participants packet for F1 2023. List of participants in the race.
    /// Frequency: Every 5 seconds
    /// Size: 1306 bytes
    /// Version: 1
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketParticipantsData
    {
        public PacketHeader m_header;            // Header

        public byte m_numActiveCars;             // Number of active cars in the data - should match number of cars on HUD

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public ParticipantData[] m_participants;
    }
}
