using System.Runtime.InteropServices;
using GamesDat.Core.Attributes;

namespace GamesDat.Core.Telemetry.Sources.AssettoCorsa
{
    /// <summary>
    /// ACC Physics data from acpmf_physics memory-mapped file
    /// Updated at ~100Hz
    /// </summary>
    [GameId("ACC")]
    [DataVersion(1, 0, 0)]
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public unsafe struct ACCPhysics
    {
        public int PacketId;
        public float Gas;
        public float Brake;
        public float Fuel;
        public int Gear;
        public int RPM;
        public float SteerAngle;
        public float SpeedKmh;

        // Velocity (world space)
        public float VelocityX;
        public float VelocityY;
        public float VelocityZ;

        // Angular velocity
        public float AngularVelX;
        public float AngularVelY;
        public float AngularVelZ;

        // G-forces
        public float AccG_X;
        public float AccG_Y;
        public float AccG_Z;

        // Wheel slip (0-1) - 4 wheels
        public fixed float WheelSlip[4];

        // Wheel load (N) - 4 wheels
        public fixed float WheelLoad[4];

        // Wheel pressure (PSI) - 4 wheels
        public fixed float WheelsPressure[4];

        // Wheel angular speed (rad/s) - 4 wheels
        public fixed float WheelAngularSpeed[4];

        // Tire wear (0-1) - 4 wheels
        public fixed float TyreWear[4];

        // Tire dirt level (0-5) - 4 wheels
        public fixed float TyreDirtyLevel[4];

        // Tire core temp (C) - 4 wheels
        public fixed float TyreCoreTemperature[4];

        // Camber angle (rad) - 4 wheels
        public fixed float CamberRAD[4];

        // Suspension travel (m) - 4 wheels
        public fixed float SuspensionTravel[4];

        public float Drs;
        public float TC;
        public float Heading;
        public float Pitch;
        public float Roll;
        public float CgHeight;

        // Car damage - 5 parts
        public fixed float CarDamage[5];

        public int NumberOfTyresOut;
        public int PitLimiterOn;
        public float Abs;

        public float KersCharge;
        public float KersInput;
        public int AutoShifterOn;

        // Ride height - 2 values (front/rear)
        public fixed float RideHeight[2];

        public float TurboBoost;
        public float Ballast;
        public float AirDensity;

        public float AirTemp;
        public float RoadTemp;

        // Local angular velocity - 3 axes
        public fixed float LocalAngularVel[3];

        public float FinalFF;

        public float PerformanceMeter;
        public int EngineBrake;
        public int ErsRecoveryLevel;
        public int ErsPowerLevel;
        public int ErsHeatCharging;
        public int ErsIsCharging;
        public float KersCurrentKJ;

        public int DrsAvailable;
        public int DrsEnabled;

        // Brake temp - 4 wheels
        public fixed float BrakeTemp[4];

        public float Clutch;

        // Tire temps (inner, middle, outer) - 4 wheels each
        public fixed float TyreTempI[4];
        public fixed float TyreTempM[4];
        public fixed float TyreTempO[4];

        public int IsAIControlled;

        // Tire contact points/normals/headings - 4 wheels each
        public fixed float TyreContactPoint[4];
        public fixed float TyreContactNormal[4];
        public fixed float TyreContactHeading[4];

        public float BrakeBias;

        // Local velocity - 3 axes
        public fixed float LocalVelocity[3];
    }
}
