# Creating New Game Integrations

This guide walks you through adding support for a new game to GameTelemetry.

## Overview

A game integration consists of:

1. **Data structures** - Represent the game's telemetry format
2. **Source implementation** - Adapts data to `ITelemetrySource<T>`
3. **Factory methods** - Convenience methods for users

## Step-by-Step Guide

### 1. Create Game Project

```bash
dotnet new classlib -n GameTelemetry.Games.YourGame
cd GameTelemetry.Games.YourGame
dotnet add reference ../GameTelemetry.Core
```

### 2. Define Data Structures

#### For Memory-Mapped Files

Use `unsafe struct` with fixed-size arrays:

```csharp
using System.Runtime.InteropServices;

namespace GameTelemetry.Games.YourGame;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public unsafe struct YourGameTelemetry
{
    public float Speed;
    public float RPM;
    public int Gear;

    // Fixed-size arrays for wheels
    public fixed float WheelSpeeds[4];

    // Strings as fixed char arrays
    public fixed char TrackName[64];

    // Helper method to read string
    public string GetTrackName()
    {
        fixed (char* ptr = TrackName)
        {
            return new string(ptr);
        }
    }
}
```

**Key rules:**

- Use `StructLayout` with explicit packing
- Arrays must be `fixed` (not managed arrays)
- No reference types (no `string`, `byte[]`, etc.)
- Must match game's memory layout exactly

#### For File-Based Sources

Use regular classes/structs - no unsafe requirements:

```csharp
public class ReplayMetadata
{
    public string FileName { get; set; }
    public DateTime CreatedAt { get; set; }
    public long FileSize { get; set; }
}
```

### 3. Create Source Factory

```csharp
using GameTelemetry.Core;

namespace GameTelemetry.Games.YourGame;

public static class YourGameSources
{
    /// <summary>
    /// Create a source for memory-mapped telemetry
    /// </summary>
    public static MemoryMappedFileSource<YourGameTelemetry> CreateTelemetrySource() =>
        new MemoryMappedFileSource<YourGameTelemetry>(
            mapName: "YourGameSharedMemory",  // Find this in game's documentation
            pollInterval: TimeSpan.FromMilliseconds(10)  // 100Hz
        );

    /// <summary>
    /// Create a source for replay file monitoring
    /// </summary>
    public static FileWatcherSource CreateReplaySource(string? customPath = null)
    {
        var path = customPath ?? GetDefaultReplayPath();

        return new FileWatcherSource(
            path: path,
            pattern: "*.replay",
            includeSubdirectories: false,
            debounceDelay: TimeSpan.FromSeconds(2)
        );
    }

    private static string GetDefaultReplayPath()
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(documents, "YourGame", "Replays");
    }
}
```

### 4. Test Your Integration

```csharp
// In GameTelemetry.TestApp/Program.cs
using GameTelemetry.Games.YourGame;

await using var session = new GameSession()
    .AddSource(YourGameSources.CreateTelemetrySource())
    .OnData<YourGameTelemetry>(data =>
        Console.WriteLine($"Speed: {data.Speed}"));

await session.StartAsync();
```

**Testing checklist:**

- [ ] Source starts without errors
- [ ] Data is captured at expected rate
- [ ] File grows during capture
- [ ] Can be read back successfully
- [ ] Data values make sense

### 5. Document Your Integration

Add to main README:

```markdown
## Supported Games

- **Your Game** - Telemetry description
```

Create example in your game's README:

```markdown
# YourGame Integration

## Memory Map Name

The game exposes telemetry via shared memory: `YourGameSharedMemory`

## Data Rate

~100Hz (10ms polling)

## Example

[Add example code here]
```

## Common Patterns

### Multiple Memory-Mapped Files

If a game has multiple MMFs (like ACC):

```csharp
public static class YourGameSources
{
    public static MemoryMappedFileSource<YourGamePhysics> CreatePhysicsSource() =>
        new("YourGame_Physics", TimeSpan.FromMilliseconds(10));

    public static MemoryMappedFileSource<YourGameGraphics> CreateGraphicsSource() =>
        new("YourGame_Graphics", TimeSpan.FromMilliseconds(100));
}

// Usage - capture both
await using var session = new GameSession()
    .AddSource(YourGameSources.CreatePhysicsSource())
    .AddSource(YourGameSources.CreateGraphicsSource());
```

### Different Poll Rates

Match the game's update frequency:

- Physics: 100-1000 Hz → `TimeSpan.FromMilliseconds(1-10)`
- Graphics: 10-60 Hz → `TimeSpan.FromMilliseconds(16-100)`
- Static: Once per session → `TimeSpan.FromSeconds(5)`

### Finding Memory Map Names

#### Method 1: Game Documentation

Check official modding/plugin documentation

#### Method 2: Tools

- **WinObj (Sysinternals)** - Browse `\BaseNamedObjects\`
- **Process Explorer** - View handles for game process

#### Method 3: Reverse Engineering

Use tools like Cheat Engine (legal for single-player analysis)

## Troubleshooting

### "Memory-mapped file not found"

- Game isn't running
- Game doesn't expose telemetry
- Wrong MMF name
- Telemetry disabled in game settings

### Data is always zero

- Polling too fast (try slower interval)
- Wrong struct layout (check packing, alignment)
- Game hasn't initialized data yet

### Struct size mismatch

```csharp
// Check actual size
Console.WriteLine($"Struct size: {Marshal.SizeOf<YourGameTelemetry>()}");

// If wrong, adjust Pack value or field order
[StructLayout(LayoutKind.Sequential, Pack = 1)]  // Try different packing
```

### Performance issues

- Polling too frequently → Increase interval
- Struct too large → Split into multiple sources
- Too many callbacks → Debounce or sample

## Need Help?

- Check existing game integrations for examples
- Open an issue with "Integration: YourGame" tag
- Join discussions for architecture questions

## Best Practices

✅ **DO:**

- Use descriptive struct field names
- Add XML comments to factory methods
- Match game's native data rates
- Handle game not running gracefully
- Provide sensible defaults

❌ **DON'T:**

- Allocate in hot paths
- Block or sleep in callbacks
- Throw exceptions from sources
- Hardcode file paths without fallbacks
- Skip documentation

## Examples

See existing integrations:

- **Simple:** [Rocket League](../GameTelemetry.Games.RocketLeague) - File watching
- **Complex:** [ACC](../GameTelemetry.Games.ACC) - Multiple MMFs with unsafe structs
