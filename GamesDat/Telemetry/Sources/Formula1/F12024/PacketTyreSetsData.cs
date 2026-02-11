using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12024
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketTyreSetsData
    {
        public PacketHeader m_header;

        public byte m_carIdx;                           // Index of the car this data relates to

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public TyreSetData[] m_tyreSetData;   // 13 (dry) + 7 (wet)

        public byte m_fittedIdx;                        // Index into array of fitted tyre
    }
}
