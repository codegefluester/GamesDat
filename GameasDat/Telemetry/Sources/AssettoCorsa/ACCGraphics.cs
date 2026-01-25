using System.Runtime.InteropServices;

namespace GameasDat.Core.Telemetry.Sources.AssettoCorsa
{
    /// <summary>
    /// ACC Graphics data from acpmf_graphics memory-mapped file
    /// Updated at ~10Hz
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public unsafe struct ACCGraphics
    {
        public int PacketId;
        public int Status;
        public int Session;

        public fixed char CurrentTime[15];
        public fixed char LastTime[15];
        public fixed char BestTime[15];
        public fixed char Split[15];

        public int CompletedLaps;
        public int Position;
        public int ICurrentTime;
        public int ILastTime;
        public int IBestTime;
        public float SessionTimeLeft;
        public float DistanceTraveled;

        public int IsInPit;
        public int CurrentSectorIndex;
        public int LastSectorTime;
        public int NumberOfLaps;

        public fixed char TyreCompound[33];

        public float ReplayTimeMultiplier;
        public float NormalizedCarPosition;

        public int ActiveCars;

        // Car coordinates - 60 cars * 3 coordinates
        public fixed float CarCoordinates[180];

        // Car IDs - 60 cars
        public fixed int CarID[60];

        public int PlayerCarID;
        public float PenaltyTime;
        public int Flag;

        public int Penalty;
        public int IdealLineOn;
        public int IsInPitLane;

        public float SurfaceGrip;
        public int MandatoryPitDone;

        public float WindSpeed;
        public float WindDirection;

        public int IsSetupMenuVisible;

        public int MainDisplayIndex;
        public int SecondaryDisplayIndex;
        public int TC;
        public int TCCut;
        public int EngineMap;
        public int ABS;
        public float FuelXLap;
        public int RainLights;
        public int FlashingLights;
        public int LightsStage;
        public float ExhaustTemperature;
        public int WiperLV;
        public int DriverStintTotalTimeLeft;
        public int DriverStintTimeLeft;
        public int RainTyres;
    }
}
