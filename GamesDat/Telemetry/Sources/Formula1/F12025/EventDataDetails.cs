using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Explicit)]
    public struct EventDataDetails
    {
        [FieldOffset(0)] public FastestLapData FastestLap;
        [FieldOffset(0)] public RetirementData Retirement;
        [FieldOffset(0)] public DRSDisabledData DRSDisabled;
        [FieldOffset(0)] public TeamMateInPitsData TeamMateInPits;
        [FieldOffset(0)] public RaceWinnerData RaceWinner;
        [FieldOffset(0)] public PenaltyData Penalty;
        [FieldOffset(0)] public SpeedTrapData SpeedTrap;
        [FieldOffset(0)] public StartLightsData StartLights;
        [FieldOffset(0)] public DriveThroughPenaltyServedData DriveThroughPenaltyServed;
        [FieldOffset(0)] public StopGoPenaltyServedData StopGoPenaltyServed;
        [FieldOffset(0)] public FlashbackData Flashback;
        [FieldOffset(0)] public ButtonsData Buttons;
        [FieldOffset(0)] public OvertakeData Overtake;
        [FieldOffset(0)] public SafetyCarData SafetyCar;
        [FieldOffset(0)] public CollisionData Collision;
        [FieldOffset(0)] public StartLightsData LightsOut;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FastestLapData
    {
        public byte VehicleIdx;
        public float LapTime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RetirementData
    {
        public byte VehicleIdx;
        public byte Reason;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TeamMateInPitsData
    {
        public byte VehicleIdx;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DRSDisabledData
    {
        public byte Reason; // 0 = Wet track, 1 = Safety car, 2 = Red flag, 3 = Min lap not reached
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RaceWinnerData
    {
        public byte VehicleIdx;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PenaltyData
    {
        public byte PenaltyType;
        public byte InfringementType;
        public byte VehicleIdx;
        public byte OtherVehicleIdx;
        public byte Time;
        public byte LapNum;
        public byte PlacesGained;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpeedTrapData
    {
        public byte VehicleIdx;
        public float Speed;
        public byte OverallFastestInSession;
        public byte DriverFastestInSession;
        public byte FastestVehicleIdxInSession;
        public float FastestSpeedInSession;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StartLightsData
    {
        public byte NumLights;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DriveThroughPenaltyServedData
    {
        public byte VehicleIdx;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StopGoPenaltyServedData
    {
        public byte VehicleIdx;
        public float StopTime; // Time spent serving stop-go in seconds
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FlashbackData
    {
        public uint FlashbackFrameIdentifier; // Changed from byte to uint
        public float FlashbackSessionTime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ButtonsData
    {
        public uint ButtonStatus;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct OvertakeData
    {
        public byte OvertakingVehicleIdx;
        public byte OvertakenVehicleIdx;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SafetyCarData
    {
        public byte SafetyCarType; // 0 = No Safety Car, 1 = Full Safety Car, 2 = Virtual Safety Car, 3 = Formation Lap Safety Car
        public byte EventType; // 0 = Deployed, 1 = Returning, 2 = Returned, 3 = Resume Race
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CollisionData
    {
        public byte Vehicle1Idx; // Changed from VehicleIdx
        public byte Vehicle2Idx; // Changed from CollidingVehicleIdx
    }
}
