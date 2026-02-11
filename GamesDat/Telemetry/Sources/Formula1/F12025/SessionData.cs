using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SessionData
    {
        public PacketHeader m_header;
        public byte m_weather;                          // Weather - 0 = clear, 1 = light cloud, 2 = overcast, 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte m_trackTemperature;                // Track temp. in degrees celsius
        public sbyte m_airTemperature;                  // Air temp. in degrees celsius
        public byte m_totalLaps;                        // Total number of laps in this race
        public ushort m_trackLength;                    // Track length in metres
        public byte m_sessionType;                      // 0 = unknown, see appendix
        public sbyte m_trackId;                         // -1 for unknown, see appendix
        public byte m_formula;                          // Formula, 0 = F1 Modern, 1 = F1 Classic, 2 = F2, 3 = F1 Generic, 4 = Beta, 6 = Esports, 8 = F1 World, 9 = F1 Elimination
        public ushort m_sessionTimeLeft;                // Time left in session in seconds
        public ushort m_sessionDuration;                // Session duration in seconds
        public byte m_pitSpeedLimit;                    // Pit speed limit in kilometres per hour
        public byte m_gamePaused;                       // Whether the game is paused - network game only
        public byte m_isSpectating;                     // Whether the player is spectating
        public byte m_spectatorCarIndex;                // Index of the car being spectated
        public byte m_sliProNativeSupport;              // SLI Pro support, 0 = inactive, 1 = active
        public byte m_numMarshalZones;                  // Number of marshal zones to follow

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
        public MarshalZone[] m_marshalZones;            // List of marshal zones - max 21

        public byte m_safetyCarStatus;                  // 0 = no safety car, 1 = full, 2 = virtual, 3 = formation lap
        public byte m_networkGame;                      // 0 = offline, 1 = online
        public byte m_numWeatherForecastSamples;        // Number of weather samples to follow

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        public WeatherForecastSample[] m_weatherForecastSamples;   // Array of weather forecast samples

        public byte m_forecastAccuracy;                 // 0 = Perfect, 1 = Approximate
        public byte m_aiDifficulty;                     // AI difficulty - 0-110
        public uint m_seasonLinkIdentifier;             // Identifier for season - persists across saves
        public uint m_weekendLinkIdentifier;            // Identifier for weekend - persists across saves
        public uint m_sessionLinkIdentifier;            // Identifier for session - persists across saves
        public byte m_pitStopWindowIdealLap;            // Ideal lap to pit on for current strategy (player)
        public byte m_pitStopWindowLatestLap;           // Latest lap to pit on for current strategy (player)
        public byte m_pitStopRejoinPosition;            // Predicted position to rejoin at (player)
        public byte m_steeringAssist;                   // 0 = off, 1 = on
        public byte m_brakingAssist;                    // 0 = off, 1 = low, 2 = medium, 3 = high
        public byte m_gearboxAssist;                    // 1 = manual, 2 = manual & suggested gear, 3 = auto
        public byte m_pitAssist;                        // 0 = off, 1 = on
        public byte m_pitReleaseAssist;                 // 0 = off, 1 = on
        public byte m_ERSAssist;                        // 0 = off, 1 = on
        public byte m_DRSAssist;                        // 0 = off, 1 = on
        public byte m_dynamicRacingLine;                // 0 = off, 1 = corners only, 2 = full
        public byte m_dynamicRacingLineType;            // 0 = 2D, 1 = 3D
        public byte m_gameMode;                         // Game mode id - see appendix
        public byte m_ruleSet;                          // Ruleset - see appendix
        public uint m_timeOfDay;                        // Local time of day - minutes since midnight
        public byte m_sessionLength;                    // 0 = None, 2 = Very Short, 3 = Short, 4 = Medium, 5 = Medium Long, 6 = Long, 7 = Full
        public byte m_speedUnitsLeadPlayer;             // 0 = MPH, 1 = KPH
        public byte m_temperatureUnitsLeadPlayer;       // 0 = Celsius, 1 = Fahrenheit
        public byte m_speedUnitsSecondaryPlayer;        // 0 = MPH, 1 = KPH
        public byte m_temperatureUnitsSecondaryPlayer;  // 0 = Celsius, 1 = Fahrenheit
        public byte m_numSafetyCarPeriods;              // Number of safety cars called during session
        public byte m_numVirtualSafetyCarPeriods;       // Number of virtual safety cars called during session
        public byte m_numRedFlagPeriods;                // Number of red flags called during session
        public byte m_equalCarPerformance;              // 0 = Off, 1 = On
        public byte m_recoveryMode;                     // 0 = None, 1 = Flashbacks, 2 = Auto-recovery
        public byte m_flashbackLimit;                   // 0 = Low, 1 = Medium, 2 = High, 3 = Unlimited
        public byte m_surfaceType;                      // 0 = Simplified, 1 = Realistic
        public byte m_lowFuelMode;                      // 0 = Easy, 1 = Hard
        public byte m_raceStarts;                       // 0 = Manual, 1 = Assisted
        public byte m_tyreTemperature;                  // 0 = Surface only, 1 = Surface & Carcass
        public byte m_pitLaneTyreSim;                   // 0 = On, 1 = Off
        public byte m_carDamage;                        // 0 = Off, 1 = Reduced, 2 = Standard, 3 = Simulation
        public byte m_carDamageRate;                    // 0 = Reduced, 1 = Standard, 2 = Simulation
        public byte m_collisions;                       // 0 = Off, 1 = Player-to-Player Off, 2 = On
        public byte m_collisionsOffForFirstLapOnly;     // 0 = Disabled, 1 = Enabled
        public byte m_mpUnsafePitRelease;               // 0 = On, 1 = Off (Multiplayer)
        public byte m_mpOffForGriefing;                 // 0 = Disabled, 1 = Enabled (Multiplayer)
        public byte m_cornerCuttingStringency;          // 0 = Regular, 1 = Strict
        public byte m_parcFermeRules;                   // 0 = Off, 1 = On
        public byte m_pitStopExperience;                // 0 = Automatic, 1 = Broadcast, 2 = Immersive
        public byte m_safetyCar;                        // 0 = Off, 1 = Reduced, 2 = Standard, 3 = Increased
        public byte m_safetyCarExperience;              // 0 = Broadcast, 1 = Immersive
        public byte m_formationLap;                     // 0 = Off, 1 = On
        public byte m_formationLapExperience;           // 0 = Broadcast, 1 = Immersive
        public byte m_redFlags;                         // 0 = Off, 1 = Reduced, 2 = Standard, 3 = Increased
        public byte m_affectsLicenceLevelSolo;          // 0 = Off, 1 = On
        public byte m_affectsLicenceLevelMP;            // 0 = Off, 1 = On
        public byte m_numSessionsInWeekend;             // Number of session in following array

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] m_weekendStructure;               // List of session types to show weekend structure - see appendix for types

        public float m_sector2LapDistanceStart;         // Distance in m around track where sector 2 starts
        public float m_sector3LapDistanceStart;         // Distance in m around track where sector 3 starts
    }
}
