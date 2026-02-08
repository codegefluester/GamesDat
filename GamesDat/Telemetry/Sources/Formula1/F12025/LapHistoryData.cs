using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LapHistoryData
    {
        public uint m_lapTimeInMS;              // Lap time in milliseconds
        public ushort m_sector1TimeMSPart;        // Sector 1 milliseconds part
        public byte m_sector1TimeMinutesPart;   // Sector 1 whole minute part
        public ushort m_sector2TimeMSPart;        // Sector 2 time milliseconds part
        public byte m_sector2TimeMinutesPart;   // Sector 2 whole minute part
        public ushort m_sector3TimeMSPart;        // Sector 3 time milliseconds part
        public byte m_sector3TimeMinutesPart;   // Sector 3 whole minute part
        public byte m_lapValidBitFlags;         // 0x01 bit set-lap valid,      0x02 bit set-sector 1 valid
                                                // 0x04 bit set-sector 2 valid, 0x08 bit set-sector 3 valid
    }
}
