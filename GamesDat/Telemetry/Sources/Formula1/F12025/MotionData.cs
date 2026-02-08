using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    /// <summary>
    /// Motion data packet for F1 2025 telemetry.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MotionData
    {
        public PacketHeader m_header;
 
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public CarMotionData[] m_carMotionData;
    }
}