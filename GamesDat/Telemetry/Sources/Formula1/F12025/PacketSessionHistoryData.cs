using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketSessionHistoryData
    {
        public PacketHeader m_header;               // Header

        // Packet specific data
        public byte m_carIdx;                   // Index of the car this lap data relates to
        public byte m_numLaps;                  // Num laps in the data (including current partial lap)
        public byte m_numTyreStints;            // Number of tyre stints in the data

        public byte m_bestLapTimeLapNum;        // Lap the best lap time was achieved on
        public byte m_bestSector1LapNum;        // Lap the best Sector 1 time was achieved on
        public byte m_bestSector2LapNum;        // Lap the best Sector 2 time was achieved on
        public byte m_bestSector3LapNum;        // Lap the best Sector 3 time was achieved on

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
        public LapHistoryData[] m_lapHistoryData;   // 100 laps of data max

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public TyreStintHistoryData[] m_tyreStintsHistoryData;
    }
}
