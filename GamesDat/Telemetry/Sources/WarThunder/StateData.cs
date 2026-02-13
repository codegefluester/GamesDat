using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using GamesDat.Core.Attributes;

namespace GamesDat.Core.Telemetry.Sources.WarThunder;

/// <summary>
/// Telemetry data from War Thunder's /state endpoint.
/// Contains primary flight/vehicle telemetry data.
/// Recommended polling rate: 60Hz.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
[GameId("WarThunder")]
[DataVersion(2, 0, 0)]
public struct StateData
{
    // Validity
    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    // Control surfaces (percent)
    [JsonPropertyName("aileron, %")]
    public float AileronPercent { get; set; }

    [JsonPropertyName("elevator, %")]
    public float ElevatorPercent { get; set; }

    [JsonPropertyName("rudder, %")]
    public float RudderPercent { get; set; }

    [JsonPropertyName("flaps, %")]
    public float FlapsPercent { get; set; }

    // Altitude and speeds
    [JsonPropertyName("H, m")]
    public float AltitudeMeters { get; set; }

    [JsonPropertyName("TAS, km/h")]
    public float TrueAirspeedKmh { get; set; }

    [JsonPropertyName("IAS, km/h")]
    public float IndicatedAirspeedKmh { get; set; }

    [JsonPropertyName("M")]
    public float Mach { get; set; }

    // Flight angles and forces
    [JsonPropertyName("AoA, deg")]
    public float AngleOfAttackDeg { get; set; }

    [JsonPropertyName("AoS, deg")]
    public float AngleOfSlipDeg { get; set; }

    [JsonPropertyName("Ny")]
    public float Ny { get; set; }

    [JsonPropertyName("Vy, m/s")]
    public float VyMs { get; set; }

    [JsonPropertyName("Wx, deg/s")]
    public float WxDegPerSec { get; set; }

    // Fuel
    [JsonPropertyName("Mfuel, kg")]
    public float FuelMassKg { get; set; }

    [JsonPropertyName("Mfuel0, kg")]
    public float FuelMassInitialKg { get; set; }

    // Gear and Airbrake
    [JsonPropertyName("gear, %")]
    public int GearPercent { get; set; }

    [JsonPropertyName("airbrake, %")]
    public int AirbrakePercent { get; set; }

    // Engine 1 parameters
    [JsonPropertyName("throttle 1, %")]
    public float Throttle1Percent { get; set; }

    [JsonPropertyName("RPM throttle 1, %")]
    public float RpmThrottle1Percent { get; set; }

    [JsonPropertyName("mixture 1, %")]
    public float Mixture1Percent { get; set; }

    [JsonPropertyName("radiator 1, %")]
    public float Radiator1Percent { get; set; }

    [JsonPropertyName("magneto 1")]
    public int Magneto1 { get; set; }

    [JsonPropertyName("power 1, hp")]
    public float Power1Hp { get; set; }

    [JsonPropertyName("RPM 1")]
    public float Rpm1 { get; set; }

    [JsonPropertyName("manifold pressure 1, atm")]
    public float ManifoldPressure1Atm { get; set; }

    [JsonPropertyName("water temp 1, C")]
    public float WaterTemp1C { get; set; }

    [JsonPropertyName("oil temp 1, C")]
    public float OilTemp1C { get; set; }

    [JsonPropertyName("pitch 1, deg")]
    public float Pitch1Deg { get; set; }

    [JsonPropertyName("thrust 1, kgs")]
    public float Thrust1Kgs { get; set; }

    [JsonPropertyName("efficiency 1, %")]
    public float Efficiency1Percent { get; set; }

    [JsonPropertyName("compressor stage 1")]
    public float CompressorStage1 { get; set; }

    // Engine 2 parameters
    [JsonPropertyName("throttle 2, %")]
    public float Throttle2Percent { get; set; }

    [JsonPropertyName("RPM throttle 2, %")]
    public float RpmThrottle2Percent { get; set; }

    [JsonPropertyName("mixture 2, %")]
    public float Mixture2Percent { get; set; }

    [JsonPropertyName("radiator 2, %")]
    public float Radiator2Percent { get; set; }

    [JsonPropertyName("magneto 2")]
    public int Magneto2 { get; set; }

    [JsonPropertyName("power 2, hp")]
    public float Power2Hp { get; set; }

    [JsonPropertyName("RPM 2")]
    public float Rpm2 { get; set; }

    [JsonPropertyName("manifold pressure 2, atm")]
    public float ManifoldPressure2Atm { get; set; }

    [JsonPropertyName("water temp 2, C")]
    public float WaterTemp2C { get; set; }

    [JsonPropertyName("oil temp 2, C")]
    public float OilTemp2C { get; set; }

    [JsonPropertyName("pitch 2, deg")]
    public float Pitch2Deg { get; set; }

    [JsonPropertyName("thrust 2, kgs")]
    public float Thrust2Kgs { get; set; }

    [JsonPropertyName("efficiency 2, %")]
    public float Efficiency2Percent { get; set; }

    [JsonPropertyName("compressor stage 2")]
    public float CompressorStage2 { get; set; }

    // Engine 3 parameters
    [JsonPropertyName("throttle 3, %")]
    public float Throttle3Percent { get; set; }

    [JsonPropertyName("RPM throttle 3, %")]
    public float RpmThrottle3Percent { get; set; }

    [JsonPropertyName("mixture 3, %")]
    public float Mixture3Percent { get; set; }

    [JsonPropertyName("radiator 3, %")]
    public float Radiator3Percent { get; set; }

    [JsonPropertyName("magneto 3")]
    public int Magneto3 { get; set; }

    [JsonPropertyName("power 3, hp")]
    public float Power3Hp { get; set; }

    [JsonPropertyName("RPM 3")]
    public float Rpm3 { get; set; }

    [JsonPropertyName("manifold pressure 3, atm")]
    public float ManifoldPressure3Atm { get; set; }

    [JsonPropertyName("water temp 3, C")]
    public float WaterTemp3C { get; set; }

    [JsonPropertyName("oil temp 3, C")]
    public float OilTemp3C { get; set; }

    [JsonPropertyName("pitch 3, deg")]
    public float Pitch3Deg { get; set; }

    [JsonPropertyName("thrust 3, kgs")]
    public float Thrust3Kgs { get; set; }

    [JsonPropertyName("efficiency 3, %")]
    public float Efficiency3Percent { get; set; }

    [JsonPropertyName("compressor stage 3")]
    public float CompressorStage3 { get; set; }

    // Engine 4 parameters
    [JsonPropertyName("throttle 4, %")]
    public float Throttle4Percent { get; set; }

    [JsonPropertyName("RPM throttle 4, %")]
    public float RpmThrottle4Percent { get; set; }

    [JsonPropertyName("mixture 4, %")]
    public float Mixture4Percent { get; set; }

    [JsonPropertyName("radiator 4, %")]
    public float Radiator4Percent { get; set; }

    [JsonPropertyName("magneto 4")]
    public int Magneto4 { get; set; }

    [JsonPropertyName("power 4, hp")]
    public float Power4Hp { get; set; }

    [JsonPropertyName("RPM 4")]
    public float Rpm4 { get; set; }

    [JsonPropertyName("manifold pressure 4, atm")]
    public float ManifoldPressure4Atm { get; set; }

    [JsonPropertyName("water temp 4, C")]
    public float WaterTemp4C { get; set; }

    [JsonPropertyName("oil temp 4, C")]
    public float OilTemp4C { get; set; }

    [JsonPropertyName("pitch 4, deg")]
    public float Pitch4Deg { get; set; }

    [JsonPropertyName("thrust 4, kgs")]
    public float Thrust4Kgs { get; set; }

    [JsonPropertyName("efficiency 4, %")]
    public float Efficiency4Percent { get; set; }

    [JsonPropertyName("compressor stage 4")]
    public float CompressorStage4 { get; set; }
}
