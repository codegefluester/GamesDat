using GamesDat.Core.Telemetry.Sources.Formula1.F12023;
using GamesDat.Core.Telemetry.Sources.Formula1.F12024;
using GamesDat.Core.Telemetry.Sources.Formula1.F12025;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    internal static class F1PacketTypeMapper
    {
        private static readonly Dictionary<(ushort format, byte id), Type> _packetTypeMap = new()
        {
            // F1 2023
            [(2023, (byte)PacketId.Motion)] = typeof(F12023.PacketMotionData),
            [(2023, (byte)PacketId.Session)] = typeof(F12023.PacketSessionData),
            [(2023, (byte)PacketId.LapData)] = typeof(F12023.PacketLapData),
            [(2023, (byte)PacketId.Event)] = typeof(F12023.PacketEventData),
            [(2023, (byte)PacketId.Participants)] = typeof(F12023.PacketParticipantsData),
            [(2023, (byte)PacketId.CarSetups)] = typeof(F12023.PacketCarSetupData),
            [(2023, (byte)PacketId.CarTelemetry)] = typeof(F12023.PacketCarTelemetryData),
            [(2023, (byte)PacketId.CarStatus)] = typeof(F12023.PacketCarStatusData),
            [(2023, (byte)PacketId.FinalClassification)] = typeof(F12023.PacketFinalClassificationData),
            [(2023, (byte)PacketId.LobbyInfo)] = typeof(F12023.PacketLobbyInfoData),
            [(2023, (byte)PacketId.CarDamage)] = typeof(F12023.PacketCarDamageData),
            [(2023, (byte)PacketId.SessionHistory)] = typeof(F12023.PacketSessionHistoryData),
            [(2023, (byte)PacketId.TyreSets)] = typeof(F12023.PacketTyreSetsData),
            [(2023, (byte)PacketId.MotionEx)] = typeof(F12023.PacketMotionExData),

            // F1 2024
            [(2024, (byte)PacketId.Motion)] = typeof(F12024.PacketMotionData),
            [(2024, (byte)PacketId.Session)] = typeof(F12024.PacketSessionData),
            [(2024, (byte)PacketId.LapData)] = typeof(F12024.PacketLapData),
            [(2024, (byte)PacketId.Event)] = typeof(F12024.PacketEventData),
            [(2024, (byte)PacketId.Participants)] = typeof(F12024.PacketParticipantsData),
            [(2024, (byte)PacketId.CarSetups)] = typeof(F12024.PacketCarSetupData),
            [(2024, (byte)PacketId.CarTelemetry)] = typeof(F12024.PacketCarTelemetryData),
            [(2024, (byte)PacketId.CarStatus)] = typeof(F12024.PacketCarStatusData),
            [(2024, (byte)PacketId.FinalClassification)] = typeof(F12024.PacketFinalClassificationData),
            [(2024, (byte)PacketId.LobbyInfo)] = typeof(F12024.PacketLobbyInfoData),
            [(2024, (byte)PacketId.CarDamage)] = typeof(F12024.PacketCarDamageData),
            [(2024, (byte)PacketId.SessionHistory)] = typeof(F12024.PacketSessionHistoryData),
            [(2024, (byte)PacketId.TyreSets)] = typeof(F12024.PacketTyreSetsData),
            [(2024, (byte)PacketId.MotionEx)] = typeof(F12024.PacketMotionExData),
            [(2024, (byte)PacketId.TimeTrial)] = typeof(F12024.PacketTimeTrialData),

            // F1 2025
            [(2025, (byte)PacketId.Motion)] = typeof(F12025.MotionData),
            [(2025, (byte)PacketId.Session)] = typeof(F12025.SessionData),
            [(2025, (byte)PacketId.LapData)] = typeof(F12025.PacketLapData),
            [(2025, (byte)PacketId.Event)] = typeof(F12025.EventData),
            [(2025, (byte)PacketId.Participants)] = typeof(F12025.ParticipantData),
            [(2025, (byte)PacketId.CarSetups)] = typeof(F12025.CarSetupData),
            [(2025, (byte)PacketId.CarTelemetry)] = typeof(F12025.CarTelemetryData),
            [(2025, (byte)PacketId.CarStatus)] = typeof(F12025.PacketCarStatusData),
            [(2025, (byte)PacketId.FinalClassification)] = typeof(F12025.PacketFinalClassificationData),
            [(2025, (byte)PacketId.LobbyInfo)] = typeof(F12025.PacketLobbyInfoData),
            [(2025, (byte)PacketId.CarDamage)] = typeof(F12025.PacketCarDamageData),
            [(2025, (byte)PacketId.SessionHistory)] = typeof(F12025.PacketSessionHistoryData),
            [(2025, (byte)PacketId.TyreSets)] = typeof(F12025.PacketTyreSetsData),
            [(2025, (byte)PacketId.MotionEx)] = typeof(F12025.PacketMotionExData),
            [(2025, (byte)PacketId.TimeTrial)] = typeof(F12025.PacketTimeTrialData),
            [(2025, (byte)PacketId.LapPositions)] = typeof(F12025.PacketLapPositionsData)
        };

        public static Type? GetPacketType(ushort packetFormat, byte packetId)
        {
            _packetTypeMap.TryGetValue((packetFormat, packetId), out var type);
            return type;
        }
    }
}
