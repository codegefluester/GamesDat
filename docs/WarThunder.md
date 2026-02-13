# War Thunder HTTP Telemetry Integration

This document describes the War Thunder real-time telemetry integration for GamesDat.

## Overview

War Thunder provides real-time telemetry through a local HTTP REST API that runs on `localhost:8111` during active matches. This integration allows you to capture flight and vehicle data at high frequency for analysis, visualization, or live dashboards.

## Breaking Changes (v2.0.0)

**DataVersion bumped from 1.0.0 to 2.0.0**

- **`Valid` field type changed**: Both `StateData` and `IndicatorsData` now use `bool` instead of `int` for the `Valid` field
  - **Old**: `public int Valid;` (where 0 = invalid, non-zero = valid)
  - **New**: `public bool Valid;` (where `false` = invalid, `true` = valid)
  - This is a breaking change for existing session files - old recordings may not deserialize correctly
  - Update your code to check `if (data.Valid)` instead of `if (data.Valid != 0)`

## Features

- ✅ HTTP polling-based telemetry capture
- ✅ Multiple endpoints with configurable polling rates
- ✅ Automatic retry with exponential backoff
- ✅ Graceful handling when game isn't running
- ✅ Binary file output with LZ4 compression
- ✅ Real-time callbacks for live processing
- ✅ Reusable `HttpPollingSourceBase<T>` for other HTTP-based games

## Supported Endpoints

### `/state` - Primary Flight/Vehicle Telemetry
**Recommended polling rate:** 60Hz (16.67ms interval)

Contains core flight data including:
- Position (X, Y, Z)
- Velocity (Vx, Vy, Vz)
- Angular velocity (Wx, Wy, Wz)
- Flight parameters (AoA, AoS, IAS, TAS, Mach, Altitude)
- G-force (Ny)
- Engine data (Throttle, RPM, Manifold Pressure, Power)
- Control surfaces (Flaps, Gear, Airbrake)
- Navigation (Compass)
- Fuel
- Timestamp

### `/indicators` - Cockpit Instrumentation
**Recommended polling rate:** 10Hz (100ms interval)

Contains instrument panel data:
- Speed indicators
- Engine instruments (RPM, Manifold Pressure, Oil/Water Temp)
- Attitude indicator (Roll, Pitch)
- Altimeter
- Vertical speed
- Compass
- Clock

## Quick Start

### Basic Usage

```csharp
using GamesDat.Core;
using GamesDat.Core.Telemetry.Sources.WarThunder;

// Capture /state endpoint at 60Hz
await using var session = new GameSession()
    .AddSource(WarThunderSources.CreateStateSource())
    .OnData<StateData>(data =>
        Console.WriteLine($"Speed: {data.IndicatedAirspeed} km/h"))
    .AutoOutput();

await session.StartAsync();
```

### Multiple Endpoints

```csharp
// Capture both endpoints with different polling rates
await using var session = new GameSession()
    .AddSource(WarThunderSources.CreateStateSource(hz: 60))      // 60Hz
    .AddSource(WarThunderSources.CreateIndicatorsSource(hz: 10)) // 10Hz
    .OnData<StateData>(data =>
        Console.WriteLine($"[State] Alt: {data.Altitude}m"))
    .OnData<IndicatorsData>(data =>
        Console.WriteLine($"[Indicators] Oil: {data.OilTemp}°C"));

await session.StartAsync();
```

### Realtime-Only (No File Output)

```csharp
await using var session = new GameSession()
    .AddSource(WarThunderSources.CreateStateSource().RealtimeOnly())
    .OnData<StateData>(data => UpdateDashboard(data));

await session.StartAsync();
```

### Custom Configuration

```csharp
var options = new HttpPollingSourceOptions
{
    BaseUrl = "http://localhost:8111",
    EndpointPath = "/state",
    PollInterval = TimeSpan.FromMilliseconds(16.67), // ~60Hz
    MaxConsecutiveErrors = 20,
    InitialRetryDelay = TimeSpan.FromSeconds(2)
};

await using var session = new GameSession()
    .AddSource(new StateSource(options));

await session.StartAsync();
```

## Factory Methods

The `WarThunderSources` class provides convenient factory methods:

```csharp
// Create /state source with default settings (60Hz)
var stateSource = WarThunderSources.CreateStateSource();

// Create /state source with custom Hz
var stateSource = WarThunderSources.CreateStateSource(hz: 100);

// Create /state source with custom base URL
var stateSource = WarThunderSources.CreateStateSource(
    baseUrl: "http://custom-host:8111",
    hz: 60
);

// Create /indicators source (default 10Hz)
var indicatorsSource = WarThunderSources.CreateIndicatorsSource();

// Create with custom options
var stateSource = WarThunderSources.CreateStateSource(
    new HttpPollingSourceOptions { /* ... */ }
);
```

### Parameter Validation

The factory methods perform parameter validation to prevent common errors:

- **`hz` parameter**: Must be greater than 0, otherwise throws `ArgumentOutOfRangeException`
  ```csharp
  // ❌ Will throw ArgumentOutOfRangeException
  var source = WarThunderSources.CreateStateSource(hz: 0);
  var source = WarThunderSources.CreateStateSource(hz: -1);

  // ✅ Valid values
  var source = WarThunderSources.CreateStateSource(hz: 1);
  var source = WarThunderSources.CreateStateSource(hz: 60);
  var source = WarThunderSources.CreateStateSource(hz: 100);
  ```

This validation prevents divide-by-zero errors during poll interval calculation.

## Data Structures

### StateData
```csharp
public struct StateData
{
    public bool Valid;                             // Data validity flag (BREAKING: Changed from int to bool in v2.0.0)
    public float X, Y, Z;                          // Position (m)
    public float Vx, Vy, Vz;                       // Velocity (m/s)
    public float Wx, Wy, Wz;                       // Angular velocity (rad/s)
    public float AngleOfAttack, AngleOfSlip;       // Flight parameters (deg)
    public float IndicatedAirspeed, TrueAirspeed;  // Speed (km/h)
    public float Mach;                             // Mach number
    public float Altitude;                         // Altitude (m)
    public float Ny;                               // G-force
    public float Throttle, RPM;                    // Engine
    public float ManifoldPressure, Power;          // Engine
    public float Flaps, Gear, Airbrake;            // Controls
    public float Compass;                          // Navigation
    public float Fuel;                             // Fuel (kg)
    public long TimeMs;                            // Timestamp

    // Note: The actual struct contains many more engine-specific fields (up to 4 engines).
    // See StateData.cs for the complete field list.
}
```

### IndicatorsData
```csharp
public struct IndicatorsData
{
    public bool Valid;                             // Data validity flag (BREAKING: Changed from int to bool in v2.0.0)
    public string Army;                            // Vehicle category: "air", "tank", or "naval"
    public string Type;                            // Vehicle type identifier
    public float Speed, PedalPosition;
    public float RpmHour, RpmMin;
    public float ManifoldPressure;
    public float OilTemp, WaterTemp;
    public float AviaHorizonRoll, AviaHorizonPitch;
    public float AltitudeHour, AltitudeMin, Altitude10k;
    public float VerticalSpeed;
    public float Compass, Compass1;
    public float ClockHour, ClockMin, ClockSec;

    // Note: The actual struct contains many more fields (100+ properties).
    // See IndicatorsData.cs for the complete field list.
    // This type contains string fields and cannot be used with BinarySessionWriter.
}
```

## Error Handling

The implementation includes robust error handling:

### Game Not Running
When the game isn't running or you're not in a match:
- Initial connection error is logged
- Automatic retry with exponential backoff (1s → 2s → 4s → 8s → ... → 30s max)
- After 10 consecutive errors (configurable), throws `InvalidOperationException`
- **You can start recording before entering a match** - it will automatically connect once available

### Mid-Match Disconnect
If the connection drops during a match:
- Same retry logic as above
- Continues retrying automatically
- Resumes data collection when connection restored

### JSON Parse Errors
If received data is corrupted:
- Error is logged
- Frame is skipped
- Polling continues normally

### HTTP Errors (404, 500, etc.)
Server errors are treated as connection errors with retry logic.

## Configuration Options

### HttpPollingSourceOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BaseUrl` | `string` | *required* | Base URL (e.g., "http://localhost:8111") |
| `EndpointPath` | `string` | *required* | Endpoint path (e.g., "/state") |
| `PollInterval` | `TimeSpan` | 100ms | Time between polls |
| `RequestTimeout` | `TimeSpan` | 5s | HTTP request timeout |
| `MaxConsecutiveErrors` | `int` | 10 | Max errors before giving up |
| `InitialRetryDelay` | `TimeSpan` | 1s | Initial backoff delay |
| `MaxRetryDelay` | `TimeSpan` | 30s | Maximum backoff delay |
| `Headers` | `Dictionary` | null | Custom HTTP headers |
| `QueryParameters` | `Dictionary` | null | Query string parameters |

## Performance

Typical metrics at 60Hz:
- **CPU overhead:** <1%
- **Memory:** ~50MB
- **Network bandwidth:** Negligible (localhost)
- **File size:** ~15MB per hour (compressed with LZ4)

## Prerequisites

1. **War Thunder must be running**
2. **You must be in an active match** (not hangar/menu)
3. **Localhost API must be enabled** (enabled by default)

## Enabling the War Thunder API

The War Thunder localhost API is **enabled by default** in recent versions. No configuration needed.

To verify it's working:
```bash
# While in a match, open a browser or use curl:
curl http://localhost:8111/state
# Should return JSON telemetry data
```

If you get connection refused, ensure:
1. War Thunder is running
2. You're in an active match (not hangar)
3. No firewall is blocking localhost:8111

## Architecture

### Two-Layer Design

**Layer 1: Generic HTTP Polling Base**
- `HttpPollingSourceBase<T>` - Abstract base class for any HTTP polling source
- `HttpPollingSourceOptions` - Configuration object
- Handles: HTTP lifecycle, polling loop, retry logic, JSON deserialization

**Layer 2: War Thunder Implementation**
- `StateData` / `IndicatorsData` - Data structures (unmanaged structs)
- `StateSource` / `IndicatorsSource` - Concrete implementations
- `WarThunderSources` - Factory methods
- `WarThunderHttpClient` - Shared HttpClient instance

### Shared HttpClient

All War Thunder sources share a single static `HttpClient` instance to prevent socket exhaustion (Microsoft best practice).

## Extending to Other HTTP Games

The `HttpPollingSourceBase<T>` is designed to be reusable for any HTTP-based telemetry API:

```csharp
// Example: Hypothetical game with HTTP telemetry
public struct MyGameData
{
    [JsonPropertyName("speed")]
    public float Speed { get; set; }

    [JsonPropertyName("rpm")]
    public float RPM { get; set; }
}

public class MyGameSource : HttpPollingSourceBase<MyGameData>
{
    public MyGameSource(HttpPollingSourceOptions options)
        : base(options) { }
}

// Usage
var source = new MyGameSource(new HttpPollingSourceOptions
{
    BaseUrl = "http://localhost:9999",
    EndpointPath = "/telemetry",
    PollInterval = TimeSpan.FromMilliseconds(50)
});
```

## Future Extensions

Potential additions:
- `/map_obj.json` - Map objects and entities
- `/map_info.json` - Map metadata
- Combined source (multiple endpoints in single struct)
- WebSocket support (if War Thunder adds it)
- JSON source generators for zero-allocation parsing

## Troubleshooting

### "Failed to connect after 10 consecutive attempts"
- Ensure War Thunder is running
- Ensure you're in an active match (not hangar/menu)
- Check firewall settings for localhost
- Try accessing http://localhost:8111/state in a browser

### High CPU usage
- Reduce polling rate (e.g., 30Hz instead of 60Hz)
- Use `.RealtimeOnly()` if you don't need file output

### Missing data fields
- Check `Valid` field (should be `true`)
- Some fields may be aircraft-specific (e.g., gear for planes, not tanks)
- Indicators endpoint is slower and less reliable than state

### Connection drops mid-match
- Normal behavior - the source will automatically retry and reconnect
- Check network stability if it happens frequently

## Examples

See `Examples/WarThunderExample.cs` for complete working examples including:
- Basic recording
- Multi-endpoint recording
- Realtime-only monitoring
- Custom configuration

## API Reference

### WarThunderSources

Static factory class for creating War Thunder sources.

**Methods:**
- `CreateStateSource(string? baseUrl = null, int hz = 60)` - Create /state source
- `CreateIndicatorsSource(string? baseUrl = null, int hz = 10)` - Create /indicators source
- `CreateStateSource(HttpPollingSourceOptions options)` - Create with custom options
- `CreateIndicatorsSource(HttpPollingSourceOptions options)` - Create with custom options

### StateSource

Concrete source for War Thunder's `/state` endpoint.

**Constructors:**
- `StateSource(HttpPollingSourceOptions options)`
- `StateSource(string baseUrl, TimeSpan pollInterval)`

### IndicatorsSource

Concrete source for War Thunder's `/indicators` endpoint.

**Constructors:**
- `IndicatorsSource(HttpPollingSourceOptions options)`
- `IndicatorsSource(string baseUrl, TimeSpan pollInterval)`

## Credits

War Thunder API documentation and community tools:
- [War Thunder Wiki - Local API](https://wiki.warthunder.com/Local_API)
- Community telemetry tools and analysis

## See Also

- [Creating Custom Sources](CREATING_SOURCES.md)
- GameSession API Reference (coming soon)
- Performance Tuning Guide (coming soon)
