namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    public enum PacketId : byte
    {
        Motion = 0,    // Contains all motion data for player’s car – only sent while player is in control
        Session = 1,    // Data about the session – track, time left
        LapData = 2,    // Data about all the lap times of cars in the session
        Event = 3,    // Various notable events that happen during a session
        Participants = 4,    // List of participants in the session, mostly relevant for multiplayer
        CarSetups = 5,    // Packet detailing car setups for cars in the race
        CarTelemetry = 6,    // Telemetry data for all cars
        CarStatus = 7,    // Status data for all cars
        FinalClassification = 8,    // Final classification confirmation at the end of a race
        LobbyInfo = 9,    // Information about players in a multiplayer lobby
        CarDamage = 10,   // Damage status for all cars
        SessionHistory = 11,   // Lap and tyre data for session
        TyreSets = 12,   // Extended tyre set data
        MotionEx = 13,   // Extended motion data for player car
        TimeTrial = 14,   // Time Trial specific data
        LapPositions = 15,   // Lap positions on each lap so a chart can be constructed
        Max
    }
}
