using System.Runtime.InteropServices;
using System.Text;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12022
{
    /// <summary>
    /// Event packet for F1 2022 telemetry.
    /// Frequency: When the event occurs
    /// Size: 40 bytes
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketEventData
    {
        public PacketHeader m_header;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] m_eventStringCode;      // Event string code, see below

        public EventDataDetails m_eventDetails; // Event details - should be interpreted differently for each type

        /// <summary>
        /// Helper to get event code as string
        /// </summary>
        public readonly string EventCode => Encoding.ASCII.GetString(m_eventStringCode);
    }
}
