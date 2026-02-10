using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12022
{
    /// <summary>
    /// Car telemetry packet for F1 2022 telemetry.
    /// Frequency: Rate as specified in menus
    /// Size: 1347 bytes
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketCarTelemetryData
    {
        public PacketHeader m_header;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public CarTelemetryData[] m_carTelemetryData;

        public byte m_mfdPanelIndex;                // Index of MFD panel open - 255 = MFD closed
                                                    // Single player, race – 0 = Car setup, 1 = Pits
                                                    // 2 = Damage, 3 =  Engine, 4 = Temperatures
                                                    // May vary depending on game mode
        public byte m_mfdPanelIndexSecondaryPlayer; // See above
        public sbyte m_suggestedGear;               // Suggested gear for the player (1-8)
                                                    // 0 if no gear suggested
    }
}
