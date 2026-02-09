using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12023
{
    /// <summary>
    /// Session packet for F1 2023. Details about the current session.
    /// Frequency: 2 per second
    /// Size: 644 bytes
    /// Version: 1
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketSessionData
    {
        public PacketHeader m_header;               // Header

        public byte m_weather;                      // Weather - 0 = clear, 1 = light cloud, 2 = overcast
                                                    // 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte m_trackTemperature;            // Track temp. in degrees celsius
        public sbyte m_airTemperature;              // Air temp. in degrees celsius
        public byte m_totalLaps;                    // Total number of laps in this race
        public ushort m_trackLength;                // Track length in metres
        public byte m_sessionType;                  // 0 = unknown, 1 = P1, 2 = P2, 3 = P3, 4 = Short P
                                                    // 5 = Q1, 6 = Q2, 7 = Q3, 8 = Short Q, 9 = OSQ
                                                    // 10 = R, 11 = R2, 12 = R3, 13 = Time Trial
        public sbyte m_trackId;                     // -1 for unknown, see appendix
        public byte m_formula;                      // Formula, 0 = F1 Modern, 1 = F1 Classic, 2 = F2,
                                                    // 3 = F1 Generic, 4 = Beta, 5 = Supercars
                                                    // 6 = Esports, 7 = F2 2021
        public ushort m_sessionTimeLeft;            // Time left in session in seconds
        public ushort m_sessionDuration;            // Session duration in seconds
        public byte m_pitSpeedLimit;                // Pit speed limit in kilometres per hour
        public byte m_gamePaused;                   // Whether the game is paused - network game only
        public byte m_isSpectating;                 // Whether the player is spectating
        public byte m_spectatorCarIndex;            // Index of the car being spectated
        public byte m_sliProNativeSupport;          // SLI Pro support, 0 = inactive, 1 = active
        public byte m_numMarshalZones;              // Number of marshal zones to follow

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
        public MarshalZone[] m_marshalZones;        // List of marshal zones - max 21

        public byte m_safetyCarStatus;              // 0 = no safety car, 1 = full
                                                    // 2 = virtual, 3 = formation lap
        public byte m_networkGame;                  // 0 = offline, 1 = online
        public byte m_numWeatherForecastSamples;    // Number of weather samples to follow

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 56)]
        public WeatherForecastSample[] m_weatherForecastSamples;   // Array of weather forecast samples

        public byte m_forecastAccuracy;             // 0 = Perfect, 1 = Approximate
        public byte m_aiDifficulty;                 // AI Difficulty rating - 0-110
        public uint m_seasonLinkIdentifier;         // Identifier for season - persists across saves
        public uint m_weekendLinkIdentifier;        // Identifier for weekend - persists across saves
        public uint m_sessionLinkIdentifier;        // Identifier for session - persists across saves
        public byte m_pitStopWindowIdealLap;        // Ideal lap to pit on for current strategy (player)
        public byte m_pitStopWindowLatestLap;       // Latest lap to pit on for current strategy (player)
        public byte m_pitStopRejoinPosition;        // Predicted position to rejoin at (player)
        public byte m_steeringAssist;               // 0 = off, 1 = on
        public byte m_brakingAssist;                // 0 = off, 1 = low, 2 = medium, 3 = high
        public byte m_gearboxAssist;                // 1 = manual, 2 = manual & suggested gear, 3 = auto
        public byte m_pitAssist;                    // 0 = off, 1 = on
        public byte m_pitReleaseAssist;             // 0 = off, 1 = on
        public byte m_ERSAssist;                    // 0 = off, 1 = on
        public byte m_DRSAssist;                    // 0 = off, 1 = on
        public byte m_dynamicRacingLine;            // 0 = off, 1 = corners only, 2 = full
        public byte m_dynamicRacingLineType;        // 0 = 2D, 1 = 3D
        public byte m_gameMode;                     // Game mode id - see appendix
        public byte m_ruleSet;                      // Ruleset - see appendix
        public uint m_timeOfDay;                    // Local time of day - minutes since midnight
        public byte m_sessionLength;                // 0 = None, 2 = Very Short, 3 = Short, 4 = Medium
                                                    // 5 = Medium Long, 6 = Long, 7 = Full
        public byte m_speedUnitsLeadPlayer;         // 0 = MPH, 1 = KPH
        public byte m_temperatureUnitsLeadPlayer;   // 0 = Celsius, 1 = Fahrenheit
        public byte m_speedUnitsSecondaryPlayer;    // 0 = MPH, 1 = KPH
        public byte m_temperatureUnitsSecondaryPlayer;  // 0 = Celsius, 1 = Fahrenheit
        public byte m_numSafetyCarPeriods;          // Number of safety cars called during session
        public byte m_numVirtualSafetyCarPeriods;   // Number of virtual safety cars called
        public byte m_numRedFlagPeriods;            // Number of red flags called during session
    }
}
