using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    public static class EventCodes
    {
        public const string SessionStarted = "SSTA";
        public const string SessionEnded = "SEND";
        public const string FastestLap = "FTLP";
        public const string Retirement = "RTMT";
        public const string DrsEnabled = "DRSE";
        public const string DrsDisabled = "DRSD";
        public const string TeamMateInPits = "TMPT";
        public const string ChequeredFlag = "CHQF";
        public const string RaceWinner = "RCWN";
        public const string Penalty = "PENA";
        public const string SpeedTrap = "SPTP";
        public const string StartLights = "STLG";
        public const string LightsOut = "LGOT";
        public const string DriveThroughServed = "DTSV";
        public const string StopGoServed = "SGSV";
        public const string Flashback = "FLBK";
        public const string ButtonStatus = "BUTN";
        public const string RedFlag = "RDFL";
        public const string Overtake = "OVTK";
        public const string SafetyCar = "SCAR";
        public const string Collision = "COLL";
    }
}
