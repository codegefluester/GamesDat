using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12024
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TimeTrialDataSet
    {
        public byte m_carIdx;                   // Index of the car this data relates to
        public byte m_teamId;                   // Team id - see appendix
        public uint m_lapTimeInMS;              // Lap time in milliseconds
        public uint m_sector1TimeInMS;          // Sector 1 time in milliseconds
        public uint m_sector2TimeInMS;          // Sector 2 time in milliseconds
        public uint m_sector3TimeInMS;          // Sector 3 time in milliseconds
        public byte m_tractionControl;          // 0 = assist off, 1 = assist on
        public byte m_gearboxAssist;            // 0 = assist off, 1 = assist on
        public byte m_antiLockBrakes;           // 0 = assist off, 1 = assist on
        public byte m_equalCarPerformance;      // 0 = Realistic, 1 = Equal
        public byte m_customSetup;              // 0 = No, 1 = Yes
        public byte m_valid;                    // 0 = invalid, 1 = valid
    }
}
