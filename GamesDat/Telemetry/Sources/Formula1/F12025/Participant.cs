using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Participant
    {
        public byte m_aiControlled;                     // Whether the vehicle is AI (1) or Human (0) controlled
        public byte m_driverId;                         // Driver id - see appendix, 255 if network human
        public byte m_networkId;                        // Network id - unique identifier for network players
        public byte m_teamId;                           // Team id - see appendix
        public byte m_myTeam;                           // My team flag - 1 = My Team, 0 = otherwise
        public byte m_raceNumber;                       // Race number of the car
        public byte m_nationality;                      // Nationality of the driver
        
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] m_name;                           // Name of participant in UTF-8 format – null terminated
                                                        // Will be truncated with ... (U+2026) if too long
        public byte m_yourTelemetry;                    // The player's UDP setting, 0 = restricted, 1 = public
        public byte m_showOnlineNames;                  // The player's show online names setting, 0 = off, 1 = on
        public ushort m_techLevel;                      // F1 World tech level
        public byte m_platform;                         // 1 = Steam, 3 = PlayStation, 4 = Xbox, 6 = Origin, 255 = unknown
        public byte m_numColours;                       // Number of colours valid for this car

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public LiveryColour[] m_liveryColours;          // Colours for the car

        /// <summary>
        /// Gets the participant name as a decoded UTF-8 string
        /// </summary>
        public readonly string Name
        {
            get
            {
                if (m_name == null) return string.Empty;
                int length = Array.IndexOf(m_name, (byte)0);
                if (length < 0) length = m_name.Length;
                return Encoding.UTF8.GetString(m_name, 0, length);
            }
        }
    }
}
