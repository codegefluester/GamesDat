using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12023
{
    /// <summary>
    /// Event packet for F1 2023. Details of events that happen during a session.
    /// Frequency: When the event occurs
    /// Size: 45 bytes
    /// Version: 1
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketEventData
    {
        public PacketHeader m_header;               // Header

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_eventStringCode;            // Event string code, see below

        public EventDataDetails m_eventDetails;     // Event details - should be interpreted differently for each type
    }
}
