using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12022
{
    // Fastest Lap event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FastestLapData
    {
        public byte vehicleIdx;  // Vehicle index of car achieving fastest lap
        public float lapTime;    // Lap time is in seconds
    }

    // Retirement event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RetirementData
    {
        public byte vehicleIdx;  // Vehicle index of car retiring
    }

    // Team mate in pits event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TeamMateInPitsData
    {
        public byte vehicleIdx;  // Vehicle index of team mate
    }

    // Race winner event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RaceWinnerData
    {
        public byte vehicleIdx;  // Vehicle index of the race winner
    }

    // Penalty event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PenaltyData
    {
        public byte penaltyType;        // Penalty type – see Appendices
        public byte infringementType;   // Infringement type – see Appendices
        public byte vehicleIdx;         // Vehicle index of the car the penalty is applied to
        public byte otherVehicleIdx;    // Vehicle index of the other car involved
        public byte time;               // Time gained, or time spent doing action in seconds
        public byte lapNum;             // Lap the penalty occurred on
        public byte placesGained;       // Number of places gained by this
    }

    // Speed trap event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpeedTrapData
    {
        public byte vehicleIdx;                     // Vehicle index of the vehicle triggering speed trap
        public float speed;                         // Top speed achieved in kilometres per hour
        public byte isOverallFastestInSession;      // Overall fastest speed in session = 1, otherwise 0
        public byte isDriverFastestInSession;       // Fastest speed for driver in session = 1, otherwise 0
        public byte fastestVehicleIdxInSession;     // Vehicle index of the vehicle that is the fastest in this session
        public float fastestSpeedInSession;         // Speed of the vehicle that is the fastest in this session
    }

    // Start lights event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StartLightsData
    {
        public byte numLights;  // Number of lights showing
    }

    // Drive through penalty served event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DriveThroughPenaltyServedData
    {
        public byte vehicleIdx;  // Vehicle index of the vehicle serving drive through
    }

    // Stop go penalty served event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct StopGoPenaltyServedData
    {
        public byte vehicleIdx;  // Vehicle index of the vehicle serving stop go
    }

    // Flashback event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FlashbackData
    {
        public uint flashbackFrameIdentifier;  // Frame identifier flashed back to
        public float flashbackSessionTime;     // Session time flashed back to
    }

    // Buttons event details
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ButtonsData
    {
        public uint m_buttonStatus;  // Bit flags specifying which buttons are being pressed currently - see appendices
    }

    /// <summary>
    /// Union of all possible event data details.
    /// The event details packet is different for each type of event.
    /// Make sure only the correct type is interpreted based on m_eventStringCode.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct EventDataDetails
    {
        [FieldOffset(0)] public FastestLapData FastestLap;
        [FieldOffset(0)] public RetirementData Retirement;
        [FieldOffset(0)] public TeamMateInPitsData TeamMateInPits;
        [FieldOffset(0)] public RaceWinnerData RaceWinner;
        [FieldOffset(0)] public PenaltyData Penalty;
        [FieldOffset(0)] public SpeedTrapData SpeedTrap;
        [FieldOffset(0)] public StartLightsData StartLights;
        [FieldOffset(0)] public DriveThroughPenaltyServedData DriveThroughPenaltyServed;
        [FieldOffset(0)] public StopGoPenaltyServedData StopGoPenaltyServed;
        [FieldOffset(0)] public FlashbackData Flashback;
        [FieldOffset(0)] public ButtonsData Buttons;
    }
}
