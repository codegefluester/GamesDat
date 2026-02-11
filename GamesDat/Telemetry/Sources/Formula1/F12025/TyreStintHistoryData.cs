using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TyreStintHistoryData
    {
        public byte m_endLap;               // Lap the tyre usage ends on (255 if current tyre)
        public byte m_tyreActualCompound;   // Actual tyres used by this driver
        public byte m_tyreVisualCompound;   // Visual tyres used by this driver
    }
}
