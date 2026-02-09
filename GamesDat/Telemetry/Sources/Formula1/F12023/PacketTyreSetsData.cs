using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12023
{
    /// <summary>
    /// Tyre sets packet for F1 2023. In-depth details about tyre sets assigned to a vehicle.
    /// Frequency: 20 per second but cycling through cars
    /// Size: 231 bytes
    /// Version: 1
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketTyreSetsData
    {
        public PacketHeader m_header;            // Header

        public byte m_carIdx;                    // Index of the car this data relates to

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public TyreSetData[] m_tyreSetData;      // 13 (dry) + 7 (wet)

        public byte m_fittedIdx;                 // Index into array of fitted tyre
    }
}
