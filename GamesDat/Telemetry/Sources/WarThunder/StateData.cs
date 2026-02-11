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
[DataVersion(1, 0, 0)]
public struct StateData
{
    // Validity
    [JsonPropertyName("valid")]
    public int Valid { get; set; }

    // Position (meters)
    [JsonPropertyName("X")]
    public float X { get; set; }

    [JsonPropertyName("Y")]
    public float Y { get; set; }

    [JsonPropertyName("Z")]
    public float Z { get; set; }

    // Velocity (m/s)
    [JsonPropertyName("Vx")]
    public float Vx { get; set; }

    [JsonPropertyName("Vy")]
    public float Vy { get; set; }

    [JsonPropertyName("Vz")]
    public float Vz { get; set; }

    // Angular velocity (rad/s)
    [JsonPropertyName("Wx")]
    public float Wx { get; set; }

    [JsonPropertyName("Wy")]
    public float Wy { get; set; }

    [JsonPropertyName("Wz")]
    public float Wz { get; set; }

    // Flight data
    [JsonPropertyName("AoA")]
    public float AngleOfAttack { get; set; }

    [JsonPropertyName("AoS")]
    public float AngleOfSlip { get; set; }

    [JsonPropertyName("IAS")]
    public float IndicatedAirspeed { get; set; }

    [JsonPropertyName("TAS")]
    public float TrueAirspeed { get; set; }

    [JsonPropertyName("M")]
    public float Mach { get; set; }

    [JsonPropertyName("H")]
    public float Altitude { get; set; }

    // G-force
    [JsonPropertyName("Ny")]
    public float Ny { get; set; }

    // Engine
    [JsonPropertyName("throttle")]
    public float Throttle { get; set; }

    [JsonPropertyName("RPM")]
    public float RPM { get; set; }

    [JsonPropertyName("manifold_pressure")]
    public float ManifoldPressure { get; set; }

    [JsonPropertyName("power")]
    public float Power { get; set; }

    // Controls
    [JsonPropertyName("flaps")]
    public float Flaps { get; set; }

    [JsonPropertyName("gear")]
    public float Gear { get; set; }

    [JsonPropertyName("airbrake")]
    public float Airbrake { get; set; }

    // Navigation
    [JsonPropertyName("compass")]
    public float Compass { get; set; }

    // Fuel
    [JsonPropertyName("fuel")]
    public float Fuel { get; set; }

    // Timestamp
    [JsonPropertyName("time")]
    public long TimeMs { get; set; }
}
