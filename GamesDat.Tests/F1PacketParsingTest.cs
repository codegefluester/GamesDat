using System.Runtime.InteropServices;
using GamesDat.Core.Telemetry.Sources.Formula1;
using GamesDat.Core.Telemetry.Sources.Formula1.F12025;

namespace GamesDat.Tests;

public class F1PacketParsingTest
{
    [Theory]
    [InlineData("Fixtures/F1/2025/packet-0.bin", 2025, PacketId.Motion, typeof(Core.Telemetry.Sources.Formula1.F12025.MotionData))]
    [InlineData("Fixtures/F1/2025/packet-1.bin", 2025, PacketId.Session, typeof(Core.Telemetry.Sources.Formula1.F12025.SessionData))]
    [InlineData("Fixtures/F1/2025/packet-2.bin", 2025, PacketId.LapData, typeof(Core.Telemetry.Sources.Formula1.F12025.PacketLapData))]
    [InlineData("Fixtures/F1/2025/packet-4.bin", 2025, PacketId.Participants, typeof(Core.Telemetry.Sources.Formula1.F12025.ParticipantData))]
    [InlineData("Fixtures/F1/2025/packet-5.bin", 2025, PacketId.CarSetups, typeof(Core.Telemetry.Sources.Formula1.F12025.CarSetupData))]
    [InlineData("Fixtures/F1/2025/packet-6.bin", 2025, PacketId.CarTelemetry, typeof(Core.Telemetry.Sources.Formula1.F12025.CarTelemetryData))]
    [InlineData("Fixtures/F1/2025/packet-7.bin", 2025, PacketId.CarStatus, typeof(Core.Telemetry.Sources.Formula1.F12025.PacketCarStatusData))]
    [InlineData("Fixtures/F1/2025/packet-8.bin", 2025, PacketId.FinalClassification, typeof(Core.Telemetry.Sources.Formula1.F12025.PacketFinalClassificationData))]
    [InlineData("Fixtures/F1/2025/packet-10.bin", 2025, PacketId.CarDamage, typeof(Core.Telemetry.Sources.Formula1.F12025.PacketCarDamageData))]
    [InlineData("Fixtures/F1/2025/packet-11.bin", 2025, PacketId.SessionHistory, typeof(Core.Telemetry.Sources.Formula1.F12025.PacketSessionHistoryData))]
    [InlineData("Fixtures/F1/2025/packet-12.bin", 2025, PacketId.TyreSets, typeof(Core.Telemetry.Sources.Formula1.F12025.PacketTyreSetsData))]
    [InlineData("Fixtures/F1/2025/packet-13.bin", 2025, PacketId.MotionEx, typeof(Core.Telemetry.Sources.Formula1.F12025.PacketMotionExData))]
    [InlineData("Fixtures/F1/2025/packet-14.bin", 2025, PacketId.TimeTrial, typeof(Core.Telemetry.Sources.Formula1.F12025.PacketTimeTrialData))]
    [InlineData("Fixtures/F1/2025/packet-15.bin", 2025, PacketId.LapPositions, typeof(Core.Telemetry.Sources.Formula1.F12025.PacketLapPositionsData))]
    public void TestParsesPackets_2025(string fixturePath, int expectedPacketFormat, PacketId expectedPacketType, Type expectedPacketTypeStruct)
    {
        using (var reader = new BinaryReader(File.OpenRead(fixturePath)))
        {
            var packet = RawDataToObject(reader.ReadBytes((int)reader.BaseStream.Length), expectedPacketTypeStruct);
            
            Assert.IsType(expectedPacketTypeStruct, packet);
            Assert.Equal(expectedPacketFormat, ((dynamic)packet).m_header.m_packetFormat);
            Assert.Equal(expectedPacketType, (PacketId)((dynamic)packet).m_header.m_packetId);
        }
    }

    [Theory]
    [InlineData("Fixtures/F1/2024/packet-0.bin", 2024, PacketId.Motion, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketMotionData))]
    [InlineData("Fixtures/F1/2024/packet-1.bin", 2024, PacketId.Session, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketSessionData))]
    [InlineData("Fixtures/F1/2024/packet-2.bin", 2024, PacketId.LapData, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketLapData))]
    [InlineData("Fixtures/F1/2024/packet-4.bin", 2024, PacketId.Participants, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketParticipantsData))]
    [InlineData("Fixtures/F1/2024/packet-5.bin", 2024, PacketId.CarSetups, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketCarSetupData))]
    [InlineData("Fixtures/F1/2024/packet-6.bin", 2024, PacketId.CarTelemetry, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketCarTelemetryData))]
    [InlineData("Fixtures/F1/2024/packet-7.bin", 2024, PacketId.CarStatus, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketCarStatusData))]
    [InlineData("Fixtures/F1/2024/packet-10.bin", 2024, PacketId.CarDamage, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketCarDamageData))]
    [InlineData("Fixtures/F1/2024/packet-11.bin", 2024, PacketId.SessionHistory, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketSessionHistoryData))]
    [InlineData("Fixtures/F1/2024/packet-12.bin", 2024, PacketId.TyreSets, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketTyreSetsData))]
    [InlineData("Fixtures/F1/2024/packet-13.bin", 2024, PacketId.MotionEx, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketMotionExData))]
    [InlineData("Fixtures/F1/2024/packet-14.bin", 2024, PacketId.TimeTrial, typeof(Core.Telemetry.Sources.Formula1.F12024.PacketTimeTrialData))]
    public void TestParsesPackets_2024(string fixturePath, int expectedPacketFormat, PacketId expectedPacketType, Type expectedPacketTypeStruct)
    {
        using (var reader = new BinaryReader(File.OpenRead(fixturePath)))
        {
            var packet = RawDataToObject(reader.ReadBytes((int)reader.BaseStream.Length), expectedPacketTypeStruct);

            Assert.IsType(expectedPacketTypeStruct, packet);
            Assert.Equal(expectedPacketFormat, ((dynamic)packet).m_header.m_packetFormat);
            Assert.Equal(expectedPacketType, (PacketId)((dynamic)packet).m_header.m_packetId);
        }
    }

    [Theory]
    [InlineData("Fixtures/F1/2023/packet-0.bin", 2023, PacketId.Motion, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketMotionData))]
    [InlineData("Fixtures/F1/2023/packet-1.bin", 2023, PacketId.Session, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketSessionData))]
    [InlineData("Fixtures/F1/2023/packet-2.bin", 2023, PacketId.LapData, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketLapData))]
    [InlineData("Fixtures/F1/2023/packet-4.bin", 2023, PacketId.Participants, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketParticipantsData))]
    [InlineData("Fixtures/F1/2023/packet-5.bin", 2023, PacketId.CarSetups, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketCarSetupData))]
    [InlineData("Fixtures/F1/2023/packet-6.bin", 2023, PacketId.CarTelemetry, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketCarTelemetryData))]
    [InlineData("Fixtures/F1/2023/packet-7.bin", 2023, PacketId.CarStatus, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketCarStatusData))]
    [InlineData("Fixtures/F1/2023/packet-10.bin", 2023, PacketId.CarDamage, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketCarDamageData))]
    [InlineData("Fixtures/F1/2023/packet-11.bin", 2023, PacketId.SessionHistory, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketSessionHistoryData))]
    [InlineData("Fixtures/F1/2023/packet-12.bin", 2023, PacketId.TyreSets, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketTyreSetsData))]
    [InlineData("Fixtures/F1/2023/packet-13.bin", 2023, PacketId.MotionEx, typeof(Core.Telemetry.Sources.Formula1.F12023.PacketMotionExData))]
    public void TestParsesPackets_2023(string fixturePath, int expectedPacketFormat, PacketId expectedPacketType, Type expectedPacketTypeStruct)
    {
        using (var reader = new BinaryReader(File.OpenRead(fixturePath)))
        {
            var packet = RawDataToObject(reader.ReadBytes((int)reader.BaseStream.Length), expectedPacketTypeStruct);

            Assert.IsType(expectedPacketTypeStruct, packet);
            Assert.Equal(expectedPacketFormat, ((dynamic)packet).m_header.m_packetFormat);
            Assert.Equal(expectedPacketType, (PacketId)((dynamic)packet).m_header.m_packetId);
        }
    }

    [Theory]
    [InlineData("Fixtures/F1/2025/packet-3-BUTN.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "BUTN")]
    [InlineData("Fixtures/F1/2025/packet-3-COLL.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "COLL")]
    [InlineData("Fixtures/F1/2025/packet-3-FLBK.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "FLBK")]
    [InlineData("Fixtures/F1/2025/packet-3-FTLP.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "FTLP")]
    [InlineData("Fixtures/F1/2025/packet-3-LGOT.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "LGOT")]
    [InlineData("Fixtures/F1/2025/packet-3-OVTK.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "OVTK")]
    [InlineData("Fixtures/F1/2025/packet-3-PENA.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "PENA")]
    [InlineData("Fixtures/F1/2025/packet-3-RCWN.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "RCWN")]
    [InlineData("Fixtures/F1/2025/packet-3-RTMT.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "RTMT")]
    [InlineData("Fixtures/F1/2025/packet-3-SEND.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "SEND")]
    [InlineData("Fixtures/F1/2025/packet-3-SPTP.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "SPTP")]
    [InlineData("Fixtures/F1/2025/packet-3-SSTA.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "SSTA")]
    [InlineData("Fixtures/F1/2025/packet-3-STLG.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "STLG")]
    [InlineData("Fixtures/F1/2025/packet-3-TMPT.bin", 2025, PacketId.Event, typeof(Core.Telemetry.Sources.Formula1.F12025.EventData), "TMPT")]
    public void TestParseEventDetails_2025(string fixturePath, int expectedPacketFormat, PacketId expectedPacketType, Type expectedPacketTypeStruct, string expectedEventCode)
    {
        using (var reader = new BinaryReader(File.OpenRead(fixturePath)))
        {
            var packet = RawDataToObject(reader.ReadBytes((int)reader.BaseStream.Length), expectedPacketTypeStruct);

            Assert.IsType(expectedPacketTypeStruct, packet);
            Assert.Equal(expectedPacketFormat, ((dynamic)packet).m_header.m_packetFormat);
            Assert.Equal(expectedPacketType, (PacketId)((dynamic)packet).m_header.m_packetId);
            Assert.Equal(expectedEventCode, ((dynamic)packet).EventCode);

            switch (expectedEventCode)
            {
                case EventCodes.ButtonStatus:
                    var buttonData = ((dynamic)packet).GetEventDetails<ButtonsData>();
                    Assert.NotNull(buttonData);
                    break;
                case EventCodes.Collision:
                    var collisionData = ((dynamic)packet).GetEventDetails<CollisionData>();
                    Assert.NotNull(collisionData);
                    break;
                case EventCodes.Flashback:
                    var flashbackData = ((dynamic)packet).GetEventDetails<FlashbackData>();
                    Assert.NotNull(flashbackData);
                    break;
                case EventCodes.FastestLap:
                    var fastestLapData = ((dynamic)packet).GetEventDetails<FastestLapData>();
                    Assert.NotNull(fastestLapData);
                    break;
                case EventCodes.StartLights:
                    var lightsOut = ((dynamic)packet).GetEventDetails<StartLightsData>();
                    Assert.NotNull(lightsOut);
                    Assert.Equal(5, lightsOut.NumLights);
                    break;
                case EventCodes.LightsOut:
                        var startLightsData = ((dynamic)packet).GetEventDetails<StartLightsData>();
                        Assert.NotNull(startLightsData);
                        Assert.Equal(0, startLightsData.NumLights);
                    break;
                case EventCodes.Overtake:
                    var overtakeData = ((dynamic)packet).GetEventDetails<OvertakeData>();
                    Assert.NotNull(overtakeData);
                    break;
                case EventCodes.Penalty:
                    var penaltyData = ((dynamic)packet).GetEventDetails<PenaltyData>();
                    Assert.NotNull(penaltyData);
                    break;
                case EventCodes.RaceWinner:
                    var raceWinnerData = ((dynamic)packet).GetEventDetails<RaceWinnerData>();
                    Assert.NotNull(raceWinnerData);
                    break;
            }
        }
    }

    public static object RawDataToObject(byte[] rawData, Type expectedPacketTypeStruct)
    {
        var pinnedRawData = GCHandle.Alloc(rawData, GCHandleType.Pinned);
        try
        {
            // Get the address of the data array
            var pinnedRawDataPtr = pinnedRawData.AddrOfPinnedObject();

            // overlay the data type on top of the raw data
            return Marshal.PtrToStructure(pinnedRawDataPtr, expectedPacketTypeStruct);
        }
        finally
        {
            // must explicitly release
            pinnedRawData.Free();
        }
    }
}
