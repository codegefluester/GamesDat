using System.Runtime.InteropServices;
using GamesDat.Core.Attributes;

namespace GamesDat.Core.Telemetry.Sources.AssettoCorsa
{
    [GameId("ACC")]
    [DataVersion(1, 0, 0)]
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public unsafe struct ACCStatic
    {
        public fixed char SMVersion[15];
        public fixed char ACVersion[15];

        public int NumberOfSessions;
        public int NumCars;

        public fixed char CarModel[33];
        public fixed char Track[33];
        public fixed char PlayerName[33];
        public fixed char PlayerSurname[33];
        public fixed char PlayerNick[33];

        public int SectorCount;
        public float MaxTorque;
        public float MaxPower;
        public int MaxRpm;
        public float MaxFuel;

        public fixed float SuspensionMaxTravel[4];
        public fixed float TyreRadius[4];

        public float MaxTurboBoost;
        public float Deprecated1;
        public float Deprecated2;

        public int PenaltiesEnabled;
        public float AidFuelRate;
        public float AidTireRate;
        public float AidMechanicalDamage;
        public int AidAllowTyreBlankets;
        public float AidStability;
        public int AidAutoClutch;
        public int AidAutoBlip;

        public int HasDRS;
        public int HasERS;
        public int HasKERS;
        public float KersMaxJ;
        public int EngineBrakeSettingsCount;
        public int ErsPowerControllerCount;
        public float TrackSPlineLength;

        public fixed char TrackConfiguration[33];

        public float ErsMaxJ;
        public int IsTimedRace;
        public int HasExtraLap;

        public fixed char CarSkin[33];

        public int ReversedGridPositions;
        public int PitWindowStart;
        public int PitWindowEnd;
        public int IsOnline;

        public fixed char DryTyresName[33];
        public fixed char WetTyresName[33];
    }
}
