using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CarTelemetryData
    {
        public PacketHeader m_header;               // Header

        // Packet specific data
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public CarTelemetry[] m_carTelemetryData;

        public byte m_mfdPanelIndex;                // Index of MFD panel open - 255 = MFD closed
                                              // Single player, race – 0 = Car setup, 1 = Pits
                                              // 2 = Damage, 3 =  Engine, 4 = Temperatures
                                              // May vary depending on game mode
        public byte m_mfdPanelIndexSecondaryPlayer; // See above
        public byte m_suggestedGear;                // Suggested gear for the player (1-8), 0 if no gear suggested

    }
}
