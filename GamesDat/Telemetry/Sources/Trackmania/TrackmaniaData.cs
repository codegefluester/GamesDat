using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Trackmania
{
    /// <summary>
    /// Trackmania telemetry data structure from ManiaPlanet_Telemetry shared memory
    /// Available in Maniaplanet 4, Trackmania (2020), and Trackmania Turbo
    /// Source: https://wiki.trackmania.io/en/third-party-tools/telemetry-api
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct TrackmaniaData
    {
        /// <summary>
        /// Version of the telemetry structure (should be 1)
        /// </summary>
        public uint Version;

        /// <summary>
        /// Current game state
        /// 0 = Menu, 1 = Running, 2 = Paused
        /// </summary>
        public uint State;

        // === Update Info ===
        /// <summary>
        /// Time when the data was last updated (milliseconds)
        /// </summary>
        public uint UpdateNumber;

        // === Race Data ===
        /// <summary>
        /// Current race time (milliseconds)
        /// </summary>
        public int RaceTime;

        /// <summary>
        /// Number of checkpoints passed
        /// </summary>
        public uint NbCheckpoints;

        /// <summary>
        /// Array of checkpoint times (milliseconds)
        /// Max 125 checkpoints
        /// </summary>
        public fixed int CheckpointTimes[125];

        // === Vehicle Info ===
        /// <summary>
        /// Vehicle position X (meters)
        /// </summary>
        public float VehiclePositionX;

        /// <summary>
        /// Vehicle position Y (meters)
        /// </summary>
        public float VehiclePositionY;

        /// <summary>
        /// Vehicle position Z (meters)
        /// </summary>
        public float VehiclePositionZ;

        /// <summary>
        /// Vehicle velocity X (m/s)
        /// </summary>
        public float VehicleVelocityX;

        /// <summary>
        /// Vehicle velocity Y (m/s)
        /// </summary>
        public float VehicleVelocityY;

        /// <summary>
        /// Vehicle velocity Z (m/s)
        /// </summary>
        public float VehicleVelocityZ;

        /// <summary>
        /// Vehicle rotation pitch (radians)
        /// </summary>
        public float VehiclePitch;

        /// <summary>
        /// Vehicle rotation yaw (radians)
        /// </summary>
        public float VehicleYaw;

        /// <summary>
        /// Vehicle rotation roll (radians)
        /// </summary>
        public float VehicleRoll;

        /// <summary>
        /// Speed in km/h
        /// </summary>
        public float Speed;

        // === Input Data ===
        /// <summary>
        /// Steering input (-1.0 to 1.0)
        /// -1.0 = full left, 1.0 = full right
        /// </summary>
        public float InputSteer;

        /// <summary>
        /// Gas input (0.0 to 1.0)
        /// </summary>
        public float InputGas;

        /// <summary>
        /// Brake input (0.0 to 1.0)
        /// </summary>
        public float InputBrake;

        /// <summary>
        /// Gear (-1 = reverse, 0 = neutral, 1+ = forward gears)
        /// </summary>
        public int Gear;

        // === Display Data ===
        /// <summary>
        /// Display speed (km/h) - what the player sees on screen
        /// </summary>
        public float DisplaySpeed;

        /// <summary>
        /// Engine RPM
        /// </summary>
        public float EngineRpm;

        // === Flags ===
        /// <summary>
        /// Boolean flags
        /// Bit 0: IsRaceRunning
        /// Bit 1: IsFinished
        /// </summary>
        public uint Flags;

        // === Player Info ===
        /// <summary>
        /// Player login (UTF-8, null-terminated)
        /// </summary>
        public fixed byte PlayerLogin[64];

        /// <summary>
        /// Player name (UTF-8, null-terminated)
        /// </summary>
        public fixed byte PlayerNickname[64];

        // === Map Info ===
        /// <summary>
        /// Map UID (UTF-8, null-terminated)
        /// </summary>
        public fixed byte MapUid[64];

        /// <summary>
        /// Map name (UTF-8, null-terminated)
        /// </summary>
        public fixed byte MapName[256];

        // === Helper Properties ===
        /// <summary>
        /// Check if the race is currently running
        /// </summary>
        public bool IsRaceRunning => (Flags & 0x1) != 0;

        /// <summary>
        /// Check if the race is finished
        /// </summary>
        public bool IsFinished => (Flags & 0x2) != 0;

        /// <summary>
        /// Get player login as string
        /// </summary>
        public string GetPlayerLogin()
        {
            fixed (byte* ptr = PlayerLogin)
            {
                return System.Text.Encoding.UTF8.GetString(ptr, 64).TrimEnd('\0');
            }
        }

        /// <summary>
        /// Get player nickname as string
        /// </summary>
        public string GetPlayerNickname()
        {
            fixed (byte* ptr = PlayerNickname)
            {
                return System.Text.Encoding.UTF8.GetString(ptr, 64).TrimEnd('\0');
            }
        }

        /// <summary>
        /// Get map UID as string
        /// </summary>
        public string GetMapUid()
        {
            fixed (byte* ptr = MapUid)
            {
                return System.Text.Encoding.UTF8.GetString(ptr, 64).TrimEnd('\0');
            }
        }

        /// <summary>
        /// Get map name as string
        /// </summary>
        public string GetMapName()
        {
            fixed (byte* ptr = MapName)
            {
                return System.Text.Encoding.UTF8.GetString(ptr, 256).TrimEnd('\0');
            }
        }
    }
}
