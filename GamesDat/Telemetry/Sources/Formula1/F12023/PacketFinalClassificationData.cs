using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12023
{
    /// <summary>
    /// Final classification packet for F1 2023. Final classification at the end of the race.
    /// Frequency: Once at the end of a race
    /// Size: 1020 bytes
    /// Version: 1
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketFinalClassificationData
    {
        public PacketHeader m_header;                      // Header

        public byte m_numCars;                             // Number of cars in the final classification

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public FinalClassificationData[] m_classificationData;
    }
}
