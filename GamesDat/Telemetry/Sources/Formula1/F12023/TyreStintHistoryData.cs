using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12023
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TyreStintHistoryData
    {
        public byte m_endLap;                // Lap the tyre usage ends on (255 of current tyre)
        public byte m_tyreActualCompound;    // Actual tyres used by this driver
        public byte m_tyreVisualCompound;    // Visual tyres used by this driver
    }
}
