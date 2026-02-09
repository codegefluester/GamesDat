using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12023
{
    /// <summary>
    /// Car status packet for F1 2023. Status data for all cars.
    /// Frequency: Rate as specified in menus
    /// Size: 1239 bytes
    /// Version: 1
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketCarStatusData
    {
        public PacketHeader m_header;               // Header

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public CarStatusData[] m_carStatusData;
    }
}
