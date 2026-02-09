using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12024
{
    /// <summary>
    /// Data about one participant in the lobby in F1 2024.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LobbyInfoData
    {
        public byte m_aiControlled;                     // Whether the vehicle is AI (1) or Human (0) controlled
        public byte m_teamId;                           // Team id - see appendix (255 if no team currently selected)
        public byte m_nationality;                      // Nationality of the driver
        public byte m_platform;                         // 1 = Steam, 3 = PlayStation, 4 = Xbox, 6 = Origin, 255 = unknown

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] m_name;     // Name of participant in UTF-8 format – null terminated
                                  // Will be truncated with ... (U+2026) if too long

        public byte m_carNumber;                        // Car number of the player
        public byte m_yourTelemetry;                    // The player's UDP setting, 0 = restricted, 1 = public
        public byte m_showOnlineNames;                  // The player's show online names setting, 0 = off, 1 = on
        public ushort m_techLevel;                      // F1 World tech level
        public byte m_readyStatus;                      // 0 = not ready, 1 = ready, 2 = spectating
    }
}
