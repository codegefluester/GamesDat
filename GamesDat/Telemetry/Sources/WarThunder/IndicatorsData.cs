using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using GamesDat.Core.Attributes;

namespace GamesDat.Core.Telemetry.Sources.WarThunder;

/// <summary>
/// Telemetry data from War Thunder's /indicators endpoint.
/// Contains cockpit/instrument panel data for all vehicle types (air, ground, naval).
/// The available fields depend on the vehicle type indicated by the 'army' field.
/// Recommended polling rate: 10Hz.
/// Note: This type contains reference fields (strings) and cannot be used with BinarySessionWriter.
/// Use realtime-only mode or a JSON-based writer for this data type.
/// </summary>
[GameId("WarThunder")]
[DataVersion(2, 0, 0)]
public struct IndicatorsData
{
    /// <summary>Default constructor to initialize string fields</summary>
    public IndicatorsData()
    {
        Army = string.Empty;
        Type = string.Empty;
    }

    // ===== Common Fields =====

    /// <summary>Data validity flag</summary>
    [JsonPropertyName("valid")]
    public bool Valid { get; set; }

    /// <summary>Vehicle category: "air", "tank", or "naval"</summary>
    [JsonPropertyName("army")]
    public string Army { get; set; } = string.Empty;

    /// <summary>Vehicle type identifier (e.g., "he51b1", "tankModels/germ_pzkpfw_35t")</summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>Vehicle speed</summary>
    [JsonPropertyName("speed")]
    public float Speed { get; set; }

    // ===== Aircraft Flight Controls =====

    [JsonPropertyName("pedals")]
    public float Pedals { get; set; }

    [JsonPropertyName("pedals1")]
    public float Pedals1 { get; set; }

    [JsonPropertyName("pedals2")]
    public float Pedals2 { get; set; }

    [JsonPropertyName("pedals3")]
    public float Pedals3 { get; set; }

    [JsonPropertyName("pedals4")]
    public float Pedals4 { get; set; }

    [JsonPropertyName("pedal_position")]
    public float PedalPosition { get; set; }

    [JsonPropertyName("stick_elevator")]
    public float StickElevator { get; set; }

    [JsonPropertyName("stick_elevator1")]
    public float StickElevator1 { get; set; }

    [JsonPropertyName("stick_ailerons")]
    public float StickAilerons { get; set; }

    // ===== Aircraft Instruments =====

    /// <summary>Vertical speed indicator</summary>
    [JsonPropertyName("vario")]
    public float Vario { get; set; }

    [JsonPropertyName("vertical_speed")]
    public float VerticalSpeed { get; set; }

    // Altimeter (three-pointer display)
    [JsonPropertyName("altitude_hour")]
    public float AltitudeHour { get; set; }

    [JsonPropertyName("altitude_min")]
    public float AltitudeMin { get; set; }

    [JsonPropertyName("altitude_10k")]
    public float Altitude10k { get; set; }

    // Artificial horizon
    [JsonPropertyName("aviahorizon_roll")]
    public float AviaHorizonRoll { get; set; }

    [JsonPropertyName("aviahorizon_pitch")]
    public float AviaHorizonPitch { get; set; }

    [JsonPropertyName("bank")]
    public float Bank { get; set; }

    [JsonPropertyName("turn")]
    public float Turn { get; set; }

    // Compass
    [JsonPropertyName("compass")]
    public float Compass { get; set; }

    [JsonPropertyName("compass1")]
    public float Compass1 { get; set; }

    [JsonPropertyName("compass2")]
    public float Compass2 { get; set; }

    // Clock
    [JsonPropertyName("clock_hour")]
    public float ClockHour { get; set; }

    [JsonPropertyName("clock_min")]
    public float ClockMin { get; set; }

    [JsonPropertyName("clock_sec")]
    public float ClockSec { get; set; }

    // G-meter
    [JsonPropertyName("g_meter")]
    public float GMeter { get; set; }

    [JsonPropertyName("g_meter_min")]
    public float GMeterMin { get; set; }

    [JsonPropertyName("g_meter_max")]
    public float GMeterMax { get; set; }

    [JsonPropertyName("aoa")]
    public float AngleOfAttack { get; set; }

    [JsonPropertyName("vne")]
    public float Vne { get; set; }

    [JsonPropertyName("mach")]
    public float Mach { get; set; }

    // ===== Aircraft Engine Instruments =====

    [JsonPropertyName("rpm")]
    public float Rpm { get; set; }

    [JsonPropertyName("rpm_hour")]
    public float RpmHour { get; set; }

    [JsonPropertyName("rpm_min")]
    public float RpmMin { get; set; }

    [JsonPropertyName("rpm1_min")]
    public float Rpm1Min { get; set; }

    [JsonPropertyName("manifold_pressure")]
    public float ManifoldPressure { get; set; }

    [JsonPropertyName("oil_pressure")]
    public float OilPressure { get; set; }

    [JsonPropertyName("oil_temp")]
    public float OilTemp { get; set; }

    [JsonPropertyName("oil_temperature")]
    public float OilTemperature { get; set; }

    [JsonPropertyName("water_temp")]
    public float WaterTemp { get; set; }

    [JsonPropertyName("water_temperature")]
    public float WaterTemperature { get; set; }

    [JsonPropertyName("head_temperature")]
    public float HeadTemperature { get; set; }

    [JsonPropertyName("carb_temperature")]
    public float CarbTemperature { get; set; }

    [JsonPropertyName("supercharger")]
    public float SuperCharger { get; set; }

    [JsonPropertyName("prop_pitch")]
    public float PropellerPitch { get; set; }

    // ===== Aircraft Controls & Systems =====

    [JsonPropertyName("throttle")]
    public float Throttle { get; set; }

    [JsonPropertyName("throttle_1")]
    public float Throttle1 { get; set; }

    [JsonPropertyName("mixture")]
    public float Mixture { get; set; }

    [JsonPropertyName("mixture_1")]
    public float Mixture1 { get; set; }

    [JsonPropertyName("radiator_lever1_1")]
    public float RadiatorLever1_1 { get; set; }

    [JsonPropertyName("fuel")]
    public float Fuel { get; set; }

    [JsonPropertyName("fuel_pressure")]
    public float FuelPressure { get; set; }

    [JsonPropertyName("flaps")]
    public float Flaps { get; set; }

    [JsonPropertyName("trimmer")]
    public float Trimmer { get; set; }

    [JsonPropertyName("airbrake_lever")]
    public float AirbrakeLever { get; set; }

    [JsonPropertyName("airbrake_indicator")]
    public float AirbrakeIndicator { get; set; }

    // Landing gear
    [JsonPropertyName("gears")]
    public float Gears { get; set; }

    [JsonPropertyName("gear")]
    public float Gear { get; set; }

    [JsonPropertyName("gears_lamp")]
    public float GearsLamp { get; set; }

    [JsonPropertyName("gear_lamp_down")]
    public float GearLampDown { get; set; }

    [JsonPropertyName("gear_lamp_up")]
    public float GearLampUp { get; set; }

    [JsonPropertyName("gear_lamp_off")]
    public float GearLampOff { get; set; }

    // Weapons
    [JsonPropertyName("weapon1")]
    public float Weapon1 { get; set; }

    [JsonPropertyName("weapon2")]
    public float Weapon2 { get; set; }

    [JsonPropertyName("weapon3")]
    public float Weapon3 { get; set; }

    [JsonPropertyName("weapon4")]
    public float Weapon4 { get; set; }

    // ===== Tank-Specific Fields =====

    [JsonPropertyName("engine_broken")]
    public float EngineBroken { get; set; }

    [JsonPropertyName("stabilizer")]
    public float Stabilizer { get; set; }

    [JsonPropertyName("gear_neutral")]
    public float GearNeutral { get; set; }

    [JsonPropertyName("has_speed_warning")]
    public float HasSpeedWarning { get; set; }

    [JsonPropertyName("driving_direction_mode")]
    public float DrivingDirectionMode { get; set; }

    [JsonPropertyName("cruise_control")]
    public float CruiseControl { get; set; }

    /// <summary>Laser Warning System</summary>
    [JsonPropertyName("lws")]
    public float Lws { get; set; }

    /// <summary>Infrared Countermeasures</summary>
    [JsonPropertyName("ircm")]
    public float Ircm { get; set; }

    [JsonPropertyName("roll_indicators_is_available")]
    public float RollIndicatorsIsAvailable { get; set; }

    [JsonPropertyName("first_stage_ammo")]
    public float FirstStageAmmo { get; set; }

    // Crew status
    [JsonPropertyName("crew_total")]
    public float CrewTotal { get; set; }

    [JsonPropertyName("crew_current")]
    public float CrewCurrent { get; set; }

    [JsonPropertyName("crew_time_to_heal")]
    public float CrewTimeToHeal { get; set; }

    [JsonPropertyName("crew_distance")]
    public float CrewDistance { get; set; }

    [JsonPropertyName("gunner_state")]
    public float GunnerState { get; set; }

    [JsonPropertyName("driver_state")]
    public float DriverState { get; set; }
}
