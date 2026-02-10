using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12022
{
    /// <summary>
    /// Participants packet for F1 2022 telemetry.
    /// Frequency: Every 5 seconds
    /// Size: 1257 bytes
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketParticipantsData
    {
        public PacketHeader m_header;

        public byte m_numActiveCars;  // Number of active cars in the data – should match number of cars on HUD

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public ParticipantData[] m_participants;
    }
}
