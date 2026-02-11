using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using GamesDat.Core.Attributes;

namespace GamesDat.Core.Telemetry.Sources.WarThunder;

/// <summary>
/// Telemetry data from War Thunder's /indicators endpoint.
/// Contains cockpit instrumentation data.
/// Recommended polling rate: 10Hz.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
[GameId("WarThunder")]
[DataVersion(1, 0, 0)]
public struct IndicatorsData
{
    // Validity
    [JsonPropertyName("valid")]
    public int Valid { get; set; }

    // Type indicator
    [JsonPropertyName("type")]
    public int Type { get; set; }

    // Speed
    [JsonPropertyName("speed")]
    public float Speed { get; set; }

    [JsonPropertyName("pedal_position")]
    public float PedalPosition { get; set; }

    // Engine instruments
    [JsonPropertyName("rpm_hour")]
    public float RpmHour { get; set; }

    [JsonPropertyName("rpm_min")]
    public float RpmMin { get; set; }

    [JsonPropertyName("manifold_pressure")]
    public float ManifoldPressure { get; set; }

    [JsonPropertyName("oil_temp")]
    public float OilTemp { get; set; }

    [JsonPropertyName("water_temp")]
    public float WaterTemp { get; set; }

    // Attitude indicator
    [JsonPropertyName("aviahorizon_roll")]
    public float AviaHorizonRoll { get; set; }

    [JsonPropertyName("aviahorizon_pitch")]
    public float AviaHorizonPitch { get; set; }

    // Altimeter
    [JsonPropertyName("altitude_hour")]
    public float AltitudeHour { get; set; }

    [JsonPropertyName("altitude_min")]
    public float AltitudeMin { get; set; }

    [JsonPropertyName("altitude_10k")]
    public float Altitude10k { get; set; }

    // Other instruments
    [JsonPropertyName("vertical_speed")]
    public float VerticalSpeed { get; set; }

    [JsonPropertyName("compass")]
    public float Compass { get; set; }

    [JsonPropertyName("compass1")]
    public float Compass1 { get; set; }

    // Clock
    [JsonPropertyName("clock_hour")]
    public float ClockHour { get; set; }

    [JsonPropertyName("clock_min")]
    public float ClockMin { get; set; }

    [JsonPropertyName("clock_sec")]
    public float ClockSec { get; set; }
}
