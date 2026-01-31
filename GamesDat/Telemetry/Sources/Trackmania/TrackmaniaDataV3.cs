using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Trackmania
{
    /// <summary>
    /// Correct Trackmania telemetry data structure (Version 3) from ManiaPlanet_Telemetry shared memory
    /// Source: https://github.com/Electron-x/TMTelemetry/blob/master/maniaplanet_telemetry.h
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct TrackmaniaDataV3
    {
        // === Game State ===
        public enum EGameState : uint
        {
            Starting = 0,
            Menus = 1,
            Running = 2,
            Paused = 3
        }

        public enum ERaceState : uint
        {
            BeforeState = 0,
            Running = 1,
            Finished = 2
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct SGameState
        {
            public EGameState State;
            public fixed byte GameplayVariant[64];  // e.g., "CarSport"
            public fixed byte MapId[64];
            public fixed byte MapName[256];
            public fixed byte _future[128];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct SRaceState
        {
            public ERaceState State;
            public uint Time;                    // Race time in milliseconds
            public uint NbRespawns;
            public uint NbCheckpoints;
            public fixed uint CheckpointTimes[125];
            public uint NbCheckpointsPerLap;
            public uint NbLapsPerRace;
            public uint Timestamp;
            public uint StartTimestamp;
            public fixed byte _future[16];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Vec3
        {
            public float X;
            public float Y;
            public float Z;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Quat
        {
            public float W;
            public float X;
            public float Y;
            public float Z;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct SObjectState
        {
            public uint Timestamp;
            public uint DiscontinuityCount;
            public Quat Rotation;
            public Vec3 Translation;            // Position
            public Vec3 Velocity;
            public uint LatestStableGroundContactTime;
            public fixed byte _future[32];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct SVehicleState
        {
            public uint Timestamp;
            public float InputSteer;
            public float InputGasPedal;
            public uint InputIsBraking;          // Bool as uint
            public uint InputIsHorn;             // Bool as uint
            public float EngineRpm;
            public int EngineCurGear;
            public float EngineTurboRatio;
            public uint EngineFreeWheeling;      // Bool as uint
            public fixed uint WheelsIsGroundContact[4];  // Bool[4] as uint[4]
            public fixed uint WheelsIsSlipping[4];       // Bool[4] as uint[4]
            public fixed float WheelsDamperLen[4];
            public float WheelsDamperRangeMin;
            public float WheelsDamperRangeMax;
            public float RumbleIntensity;
            public uint SpeedMeter;              // Speed in km/h
            public uint IsInWater;               // Bool as uint
            public uint IsSparkling;             // Bool as uint
            public uint IsLightTrails;           // Bool as uint
            public uint IsLightsOn;              // Bool as uint
            public uint IsFlying;                // Bool as uint
            public uint IsOnIce;                 // Bool as uint
            public uint Handicap;
            public float BoostRatio;
            public fixed byte _future[20];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct SDeviceState
        {
            public Vec3 Euler;
            public float CenteredYaw;
            public float CenteredAltitude;
            public fixed byte _future[32];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public unsafe struct SPlayerState
        {
            public uint IsLocalPlayer;           // Bool as uint
            public fixed byte Trigram[4];
            public fixed byte DossardNumber[4];
            public float Hue;
            public fixed byte UserName[256];
            public fixed byte _future[28];
        }

        // === Main Structure ===
        public SGameState Game;
        public SRaceState Race;
        public SObjectState Object;
        public SVehicleState Vehicle;
        public SDeviceState Device;
        public SPlayerState Player;

        // === Helper Methods ===
        public string GetGameplayVariant()
        {
            fixed (byte* ptr = Game.GameplayVariant)
            {
                return System.Text.Encoding.UTF8.GetString(ptr, 64).TrimEnd('\0');
            }
        }

        public string GetMapId()
        {
            fixed (byte* ptr = Game.MapId)
            {
                return System.Text.Encoding.UTF8.GetString(ptr, 64).TrimEnd('\0');
            }
        }

        public string GetMapName()
        {
            fixed (byte* ptr = Game.MapName)
            {
                return System.Text.Encoding.UTF8.GetString(ptr, 256).TrimEnd('\0');
            }
        }

        public string GetPlayerName()
        {
            fixed (byte* ptr = Player.UserName)
            {
                return System.Text.Encoding.UTF8.GetString(ptr, 256).TrimEnd('\0');
            }
        }

        public string GetTrigram()
        {
            fixed (byte* ptr = Player.Trigram)
            {
                return System.Text.Encoding.UTF8.GetString(ptr, 4).TrimEnd('\0');
            }
        }
    }
}
