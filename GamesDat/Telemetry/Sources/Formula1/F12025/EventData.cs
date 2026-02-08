using System.Runtime.InteropServices;
using System.Text;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EventData
    {
        public PacketHeader m_header;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] EventStringCode;

        public EventDataDetails m_eventDetails;

        // Helper property to get event code as string
        public string EventCode => Encoding.ASCII.GetString(EventStringCode);

        public T GetEventDetails<T>() where T : struct
        {
            return EventCode switch
            {
                EventCodes.FastestLap when typeof(T) == typeof(FastestLapData) => (T)(object)m_eventDetails.FastestLap,
                EventCodes.Retirement when typeof(T) == typeof(RetirementData) => (T)(object)m_eventDetails.Retirement,
                EventCodes.DrsDisabled when typeof(T) == typeof(DRSDisabledData) => (T)(object)m_eventDetails.DRSDisabled,
                EventCodes.TeamMateInPits when typeof(T) == typeof(TeamMateInPitsData) => (T)(object)m_eventDetails.TeamMateInPits,
                EventCodes.RaceWinner when typeof(T) == typeof(RaceWinnerData) => (T)(object)m_eventDetails.RaceWinner,
                EventCodes.Penalty when typeof(T) == typeof(PenaltyData) => (T)(object)m_eventDetails.Penalty,
                EventCodes.SpeedTrap when typeof(T) == typeof(SpeedTrapData) => (T)(object)m_eventDetails.SpeedTrap,
                EventCodes.StartLights when typeof(T) == typeof(StartLightsData) => (T)(object)m_eventDetails.StartLights,
                EventCodes.DriveThroughServed when typeof(T) == typeof(DriveThroughPenaltyServedData) => (T)(object)m_eventDetails.DriveThroughPenaltyServed,
                EventCodes.StopGoServed when typeof(T) == typeof(StopGoPenaltyServedData) => (T)(object)m_eventDetails.StopGoPenaltyServed,
                EventCodes.Flashback when typeof(T) == typeof(FlashbackData) => (T)(object)m_eventDetails.Flashback,
                EventCodes.ButtonStatus when typeof(T) == typeof(ButtonsData) => (T)(object)m_eventDetails.Buttons,
                EventCodes.Overtake when typeof(T) == typeof(OvertakeData) => (T)(object)m_eventDetails.Overtake,
                EventCodes.SafetyCar when typeof(T) == typeof(SafetyCarData) => (T)(object)m_eventDetails.SafetyCar,
                EventCodes.Collision when typeof(T) == typeof(CollisionData) => (T)(object)m_eventDetails.Collision,
                // fallback for unsupported event types or mismatched type requests
                _ => throw new InvalidOperationException($"Cannot get {typeof(T).Name} for event {EventCode}")
            };
        }
    }
}
