using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketTimeTrialData
    {
        public PacketHeader m_header;                       // Header

        public TimeTrialDataSet m_playerSessionBestDataSet;     // Player session best data set
        public TimeTrialDataSet m_personalBestDataSet;          // Personal best data set
        public TimeTrialDataSet m_rivalDataSet;                 // Rival data set
    }
}
