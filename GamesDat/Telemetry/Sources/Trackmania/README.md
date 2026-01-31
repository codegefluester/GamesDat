# Trackmania Telemetry

High-performance telemetry source for Trackmania games using the `ManiaPlanet_Telemetry` shared memory interface.

## Supported Games

- **Maniaplanet 4**
- **Trackmania (2020)**
- **Trackmania Turbo**

## Quick Start

```csharp
using GamesDat.Core.Telemetry.Sources.Trackmania;

// Create telemetry source
var source = TrackmaniaMemoryMappedSource.CreateTelemetrySource();

// Read telemetry continuously
await foreach (var data in source.ReadContinuousAsync(cancellationToken))
{
    Console.WriteLine($"Speed: {data.Vehicle.SpeedMeter} km/h");
    Console.WriteLine($"Position: ({data.Object.Translation.X}, {data.Object.Translation.Y}, {data.Object.Translation.Z})");
    Console.WriteLine($"Race State: {data.Race.State}");
    Console.WriteLine($"Game State: {data.Game.State}");
}
```

## Telemetry Availability

Telemetry data becomes **available immediately when Trackmania launches** - you don't need to wait for a race to start. The telemetry stream is continuous and available in all game states:

- `Starting` - Game is initializing
- `Menus` - Player is in menus
- `Running` - Actively racing or in gameplay
- `Paused` - Game is paused

You can start listening for telemetry as soon as the game process is running, regardless of whether the player is in menus, racing, or replaying.

## Important Behavioral Notes

### Post-Race Replay Telemetry

> **Warning:** After completing a race, the game displays a post-race menu with a replay playing in the background. **During this replay, telemetry continues to stream** and the data is **structurally identical** to live race telemetry.

**Key points:**

1. **Replay data is indistinguishable from live data** - There is no flag, field, or indicator that marks the telemetry as coming from a replay versus a live race.

2. **Race state transitions during replay** - When a race finishes, the `RaceState` follows this pattern:
   - `Running` → `Finished` → `Running` (replay starts)

   The return to `Running` state during the post-race menu replay can make it appear as though a new race has started.

3. **Application logic required** - If your application needs to distinguish between live racing and replays, you must implement your own tracking logic. Consider:
   - Monitoring race state transitions to detect the `Finished` state
   - Tracking when the race completion occurs and ignoring subsequent `Running` states
   - Monitoring menu state changes (though `GameState` may remain `Running`)
   - Implementing timeout logic after race completion

**Example of detecting race completion:**

```csharp
var previousRaceState = TrackmaniaDataV3.ERaceState.BeforeState;
bool raceJustFinished = false;

await foreach (var data in source.ReadContinuousAsync(cancellationToken))
{
    // Detect race completion
    if (data.Race.State == TrackmaniaDataV3.ERaceState.Finished &&
        previousRaceState == TrackmaniaDataV3.ERaceState.Running)
    {
        Console.WriteLine($"Race completed! Time: {data.Race.Time}ms");
        raceJustFinished = true;
    }

    // Ignore replay data after race completion
    if (raceJustFinished && data.Race.State == TrackmaniaDataV3.ERaceState.Running)
    {
        // This is replay data - ignore or handle differently
        continue;
    }

    // Reset flag when a new race actually starts
    if (data.Race.State == TrackmaniaDataV3.ERaceState.BeforeState)
    {
        raceJustFinished = false;
    }

    previousRaceState = data.Race.State;
}
```

## Data Structure

The telemetry data is provided through the `TrackmaniaDataV3` structure, which contains the following sections:

### Game State (`data.Game`)
- `State` - Current game state (Starting, Menus, Running, Paused)
- `GetGameplayVariant()` - Gameplay variant (e.g., "CarSport")
- `GetMapId()` - Current map identifier
- `GetMapName()` - Human-readable map name

### Race State (`data.Race`)
- `State` - Race state (BeforeState, Running, Finished)
- `Time` - Current race time in milliseconds
- `NbRespawns` - Number of respawns
- `NbCheckpoints` - Number of checkpoints passed
- `CheckpointTimes` - Array of checkpoint times

### Object State (`data.Object`)
- `Translation` - Vehicle position (Vec3)
- `Rotation` - Vehicle rotation (Quaternion)
- `Velocity` - Current velocity vector (Vec3)
- `LatestStableGroundContactTime` - Last time vehicle touched ground

### Vehicle State (`data.Vehicle`)
- `SpeedMeter` - Speed in km/h
- `InputSteer` - Steering input (-1 to 1)
- `InputGasPedal` - Gas pedal input (0 to 1)
- `InputIsBraking` - Brake input (boolean as uint)
- `EngineRpm` - Engine RPM
- `EngineCurGear` - Current gear
- `BoostRatio` - Boost level (0 to 1)
- `IsFlying`, `IsOnIce`, `IsInWater` - Surface state flags

### Device State (`data.Device`)
- `Euler` - Device orientation (Vec3)
- `CenteredYaw`, `CenteredAltitude` - Device positioning

### Player State (`data.Player`)
- `GetPlayerName()` - Player username
- `GetTrigram()` - Player trigram
- `Hue` - Player color hue

> **Note:** The complete structure definition is available in `TrackmaniaDataV3.cs`. The data structure is based on [TMTelemetry by Electron-x](https://github.com/Electron-x/TMTelemetry/blob/master/maniaplanet_telemetry.h).

## Advanced Usage

### Custom Poll Interval

The default poll rate is 16ms (~60Hz). You can customize this:

```csharp
// Poll every 8ms (~120Hz) for higher frequency data
var source = TrackmaniaMemoryMappedSource.CreateTelemetrySource(
    pollInterval: TimeSpan.FromMilliseconds(8)
);
```

### Direct Instantiation

For more control, you can directly instantiate the source:

```csharp
var source = new TrackmaniaMemoryMappedFileSource(
    mapName: TrackmaniaMemoryMappedSource.TelemetryMapName,
    pollInterval: TimeSpan.FromMilliseconds(16)
);
```

### Accessing Specific Fields

```csharp
await foreach (var data in source.ReadContinuousAsync(cancellationToken))
{
    // Game information
    string mapName = data.GetMapName();
    string gameplayVariant = data.GetGameplayVariant();

    // Vehicle dynamics
    float speed = data.Vehicle.SpeedMeter;
    int gear = data.Vehicle.EngineCurGear;
    float rpm = data.Vehicle.EngineRpm;
    float boost = data.Vehicle.BoostRatio;

    // Position and rotation
    var position = data.Object.Translation;
    var velocity = data.Object.Velocity;
    var rotation = data.Object.Rotation;

    // Race progress
    uint raceTime = data.Race.Time;
    uint checkpoints = data.Race.NbCheckpoints;
    uint respawns = data.Race.NbRespawns;

    // Inputs
    float steer = data.Vehicle.InputSteer;
    float gas = data.Vehicle.InputGasPedal;
    bool braking = data.Vehicle.InputIsBraking != 0;
}
```

## Troubleshooting

### Error: Trackmania telemetry 'ManiaPlanet_Telemetry' not found

**Cause:** The shared memory is not available.

**Solutions:**
- Ensure Trackmania is running
- Verify you're using a supported game version (Maniaplanet 4, Trackmania 2020, or Trackmania Turbo)
- The shared memory becomes available immediately when the game launches

### Error: Invalid shared memory format

**Cause:** The shared memory exists but doesn't contain the expected magic string "ManiaPlanet_Telemetry".

**Solutions:**
- Another application may have created a memory-mapped file with the same name
- Restart Trackmania to recreate the shared memory

### Error: Unsupported telemetry version

**Cause:** The game is using a different telemetry version than expected (version 3).

**Solutions:**
- Ensure you're using a compatible game version
- Check for updates to GamesDat that may support newer telemetry versions

### Warning: Data structure size mismatch

**Cause:** The size reported in the shared memory header doesn't match the compiled `TrackmaniaDataV3` structure size.

**Impact:** This is usually harmless - the structure may have padding differences or the game may report a different size. Data should still be read correctly.

## Technical Details

### Shared Memory Protocol

- **Memory name:** `ManiaPlanet_Telemetry`
- **Default poll rate:** 16ms (~60Hz)
- **Header size:** 44 bytes
- **Telemetry version:** 3

### Synchronization Protocol

The implementation uses the `UpdateNumber` field in the header for lock-free synchronization:

1. Read `UpdateNumber` before reading data
2. If `UpdateNumber` is **odd**, the game is writing - skip this read
3. Read telemetry data
4. Read `UpdateNumber` again after reading
5. If both `UpdateNumber` values are **even** and **identical**, the read is valid
6. If values differ, a torn read occurred - retry (up to 3 attempts)

This ensures data consistency without requiring locks or blocking the game's telemetry writes.

### Header Structure

The shared memory begins with a 44-byte header:

| Offset | Size | Field | Description |
|--------|------|-------|-------------|
| 0 | 22 | Magic | ASCII string "ManiaPlanet_Telemetry" |
| 22 | 10 | Padding | Reserved bytes |
| 32 | 4 | Version | Telemetry version (expected: 3) |
| 36 | 4 | Size | Size of data structure in bytes |
| 40 | 4 | UpdateNumber | Synchronization counter (even = stable, odd = writing) |

The telemetry data (`TrackmaniaDataV3`) begins at offset 44.

## References

- **Source code:** `TrackmaniaMemoryMappedSource.cs`, `TrackmaniaMemoryMappedFileSource.cs`
- **Data structure:** `TrackmaniaDataV3.cs`
- **Header format:** `TrackmaniaMemoryHeader.cs`
- **Original specification:** [TMTelemetry by Electron-x](https://github.com/Electron-x/TMTelemetry/blob/master/maniaplanet_telemetry.h)
