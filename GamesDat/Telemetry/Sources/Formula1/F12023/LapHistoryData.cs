using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12023
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LapHistoryData
    {
        public uint m_lapTimeInMS;           // Lap time in milliseconds
        public ushort m_sector1TimeInMS;     // Sector 1 time in milliseconds
        public byte m_sector1TimeMinutes;    // Sector 1 whole minute part
        public ushort m_sector2TimeInMS;     // Sector 2 time in milliseconds
        public byte m_sector2TimeMinutes;    // Sector 2 whole minute part (note: spec has typo, says sector1TimeMinutes)
        public ushort m_sector3TimeInMS;     // Sector 3 time in milliseconds
        public byte m_sector3TimeMinutes;    // Sector 3 whole minute part
        public byte m_lapValidBitFlags;      // 0x01 bit set-lap valid,      0x02 bit set-sector 1 valid
                                             // 0x04 bit set-sector 2 valid, 0x08 bit set-sector 3 valid
    }
}
