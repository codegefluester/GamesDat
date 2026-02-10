using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12022
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LapHistoryData
    {
        public uint m_lapTimeInMS;        // Lap time in milliseconds
        public ushort m_sector1TimeInMS;  // Sector 1 time in milliseconds
        public ushort m_sector2TimeInMS;  // Sector 2 time in milliseconds
        public ushort m_sector3TimeInMS;  // Sector 3 time in milliseconds
        public byte m_lapValidBitFlags;   // 0x01 bit set-lap valid,      0x02 bit set-sector 1 valid
                                          // 0x04 bit set-sector 2 valid, 0x08 bit set-sector 3 valid
    }
}
