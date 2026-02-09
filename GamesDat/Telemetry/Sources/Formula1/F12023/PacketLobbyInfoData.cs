using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12023
{
    /// <summary>
    /// Lobby info packet for F1 2023. Players currently in a multiplayer lobby.
    /// Frequency: Two every second when in the lobby
    /// Size: 1218 bytes
    /// Version: 1
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketLobbyInfoData
    {
        public PacketHeader m_header;                       // Header

        public byte m_numPlayers;                           // Number of players in the lobby data

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public LobbyInfoData[] m_lobbyPlayers;
    }
}
