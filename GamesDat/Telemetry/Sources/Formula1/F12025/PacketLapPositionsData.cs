using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketLapPositionsData
    {
        public PacketHeader m_header;                   // Header

        // Packet specific data
        public byte m_numLaps;                  // Number of laps in the data
        public byte m_lapStart;                 // Index of the lap where the data starts, 0 indexed

        // Array holding the position of the car in a given lap, 0 if no record
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50 * 22)]
        public byte[] m_positionForVehicleIdx; // [50 laps][22 cars]
    }
}
