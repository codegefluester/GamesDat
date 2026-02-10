using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12022
{
    /// <summary>
    /// Final classification packet for F1 2022 telemetry.
    /// Frequency: Once at the end of a race
    /// Size: 1015 bytes
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketFinalClassificationData
    {
        public PacketHeader m_header;

        public byte m_numCars;  // Number of cars in the final classification

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public FinalClassificationData[] m_classificationData;
    }
}
