# Usage Examples

## Basic Capture

### Simple - One Source
```csharp
using GameTelemetry.Core;
using GameTelemetry.Games.ACC;

await using var session = new GameSession()
    .AddSource(ACCSources.CreatePhysicsSource());

await session.StartAsync();
await Task.Delay(Timeout.Infinite); // Capture until Ctrl+C
```

### Multiple Sources
```csharp
await using var session = new GameSession()
    .AddSource(ACCSources.CreatePhysicsSource())
    .AddSource(ACCSources.CreateGraphicsSource())
    .AddSource(ACCSources.CreateStaticSource());

await session.StartAsync();
```

### Combined Source (Recommended for Complete Telemetry)
```csharp
// Get all ACC data (Physics, Graphics, Static) in a single object
// Benefits: 15-25% lower CPU usage, zero lock contention, complete snapshot
await using var session = new GameSession()
    .AddSource(ACCSources.CreateCombinedSource());

await session.StartAsync();
```

**When to use Combined vs Individual sources:**
- **Combined Source:** Use when you need complete telemetry data. Single writer eliminates lock contention. Physics updates at 100Hz, Graphics cached at 10Hz, Static cached at 5s intervals.
- **Individual Sources:** Use when you only need specific data (e.g., just Physics for live dashboard, or just Graphics for lap timing).

## Output Control

### Auto-Generated Filenames
```csharp
// Default - auto-generates in ./sessions/
await using var session = new GameSession()
    .AddSource(ACCSources.CreatePhysicsSource());
    
// → ./sessions/accphysics_20260125_120000.dat
```

### Custom Output Directory
```csharp
await using var session = new GameSession(defaultOutputDirectory: "./my_races")
    .AddSource(ACCSources.CreatePhysicsSource());
    
// → ./my_races/accphysics_20260125_120000.dat
```

### Explicit Paths
```csharp
var physicsSource = ACCSources.CreatePhysicsSource()
    .OutputTo("./race_20260125/physics.dat");

await using var session = new GameSession()
    .AddSource(physicsSource);
```

### No Output (Real-time Only)
```csharp
var physicsSource = ACCSources.CreatePhysicsSource()
    .RealtimeOnly();

await using var session = new GameSession()
    .AddSource(physicsSource)
    .OnData<ACCPhysics>(data =>
        Console.WriteLine($"Speed: {data.SpeedKmh}"));

await session.StartAsync();
```

## Real-time Processing

### Console Logging
```csharp
await using var session = new GameSession()
    .AddSource(ACCSources.CreatePhysicsSource())
    .OnData<ACCPhysics>(data =>
    {
        Console.WriteLine($"Speed: {data.SpeedKmh:F1} km/h | RPM: {data.RPM} | Gear: {data.Gear}");
    });

await session.StartAsync();
```

### Live Dashboard Update
```csharp
var dashboard = new LiveDashboard();

await using var session = new GameSession()
    .AddSource(ACCSources.CreatePhysicsSource())
    .OnData<ACCPhysics>(data => dashboard.Update(data));

await session.StartAsync();
```

### Combined Source Real-time Processing
```csharp
await using var session = new GameSession()
    .AddSource(ACCSources.CreateCombinedSource())
    .OnData<ACCCombinedData>(data =>
    {
        // Access all telemetry in one callback
        Console.WriteLine($"Speed: {data.Physics.SpeedKmh:F1} km/h | " +
                         $"Gear: {data.Physics.Gear} | " +
                         $"Position: {data.Graphics.Position} | " +
                         $"Lap: {data.Graphics.CompletedLaps}/{data.Graphics.NumberOfLaps}");

        // Check data staleness
        var graphicsAge = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - data.GraphicsTimestamp;
        if (graphicsAge > 150) // More than 150ms old
        {
            Console.WriteLine($"Warning: Graphics data is {graphicsAge}ms stale");
        }
    });

await session.StartAsync();
```

### Multiple Callbacks
```csharp
await using var session = new GameSession()
    .AddSource(ACCSources.CreatePhysicsSource())
    .OnData<ACCPhysics>(data => Console.WriteLine($"Speed: {data.SpeedKmh}"))
    .OnData<ACCPhysics>(data => SendToWebSocket(data))
    .OnData<ACCPhysics>(data => UpdateDatabase(data));

await session.StartAsync();
```

## File Watching

### Rocket League Replays
```csharp
using GameTelemetry.Games.RocketLeague;

var replaySource = RocketLeagueSources.CreateReplaySource()
    .RealtimeOnly();

await using var session = new GameSession()
    .AddSource(replaySource)
    .OnData<string>(replayPath =>
    {
        Console.WriteLine($"New replay: {Path.GetFileName(replayPath)}");
        // Process replay file
        UploadToCloud(replayPath);
    });

await session.StartAsync();
```

### Custom Replay Folder
```csharp
var replaySource = RocketLeagueSources.CreateReplaySource(
    customPath: @"D:\Backups\RocketLeague\Replays")
    .RealtimeOnly();

await using var session = new GameSession()
    .AddSource(replaySource)
    .OnData<string>(ProcessReplay);

await session.StartAsync();
```

## Reading Sessions

### Basic Read
```csharp
using GameTelemetry.Core;

await foreach (var (timestamp, data) in SessionReader.ReadAsync<ACCPhysics>("session.dat"))
{
    Console.WriteLine($"{timestamp}: Speed={data.SpeedKmh}");
}
```

### Analyze Session
```csharp
float maxSpeed = 0;
int frameCount = 0;

await foreach (var (timestamp, data) in SessionReader.ReadAsync<ACCPhysics>("session.dat"))
{
    maxSpeed = Math.Max(maxSpeed, data.SpeedKmh);
    frameCount++;
}

Console.WriteLine($"Max speed: {maxSpeed} km/h over {frameCount} frames");
```

### Read Combined Source Session
```csharp
await foreach (var (timestamp, data) in SessionReader.ReadAsync<ACCCombinedData>("session.dat"))
{
    // Full telemetry available in playback
    Console.WriteLine($"[{timestamp}] Speed: {data.Physics.SpeedKmh} | " +
                     $"Pos: {data.Graphics.Position} | " +
                     $"Track: {data.Static.Track}");
}
```

### Export to CSV
```csharp
using var writer = new StreamWriter("output.csv");
await writer.WriteLineAsync("Timestamp,Speed,RPM,Gear");

await foreach (var (timestamp, data) in SessionReader.ReadAsync<ACCPhysics>("session.dat"))
{
    await writer.WriteLineAsync($"{timestamp},{data.SpeedKmh},{data.RPM},{data.Gear}");
}
```

## Advanced Patterns

### Background Service
```csharp
public class TelemetryBackgroundService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var session = new GameSession()
            .AddSource(ACCSources.CreatePhysicsSource())
            .OnData<ACCPhysics>(data => ProcessTelemetry(data));

        await session.StartAsync(stoppingToken);
        
        // Keep running until service stops
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    
    private void ProcessTelemetry(ACCPhysics data)
    {
        // Your processing logic
    }
}
```

### Conditional Capture
```csharp
bool isRacing = false;

await using var session = new GameSession()
    .AddSource(ACCSources.CreateGraphicsSource())
    .OnData<ACCGraphics>(graphics =>
    {
        // Only start capturing when race begins
        if (graphics.Session == 3 && !isRacing) // 3 = race
        {
            isRacing = true;
            Console.WriteLine("Race started, now recording...");
        }
    });

await session.StartAsync();
```

### Lap Detection
```csharp
int lastLap = 0;

await using var session = new GameSession()
    .AddSource(ACCSources.CreateGraphicsSource())
    .OnData<ACCGraphics>(graphics =>
    {
        if (graphics.CompletedLaps > lastLap)
        {
            Console.WriteLine($"Lap {lastLap + 1} completed: {graphics.GetLastTime()}");
            lastLap = graphics.CompletedLaps;
        }
    });

await session.StartAsync();
```

### Combine Multiple Games
```csharp
var accSource = ACCSources.CreatePhysicsSource();
var rlSource = RocketLeagueSources.CreateReplaySource()
    .RealtimeOnly();

await using var session = new GameSession()
    .AddSource(accSource)
    .AddSource(rlSource)
    .OnData<ACCPhysics>(acc => Console.WriteLine($"ACC: {acc.SpeedKmh} km/h"))
    .OnData<string>(rl => Console.WriteLine($"RL Replay: {rl}"));

await session.StartAsync();
```

## Error Handling

### Graceful Game Not Running
```csharp
try
{
    await using var session = new GameSession()
        .AddSource(ACCSources.CreatePhysicsSource());

    await session.StartAsync();
    await Task.Delay(Timeout.Infinite);
}
catch (FileNotFoundException)
{
    Console.WriteLine("ACC is not running. Please start the game first.");
}
```

### Timeout Pattern
```csharp
var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));

await using var session = new GameSession()
    .AddSource(ACCSources.CreatePhysicsSource());

await session.StartAsync(cts.Token);

try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("30 minute capture limit reached");
}
```

## Testing Patterns

### Mock Source for Unit Tests
```csharp
public class MockTelemetrySource : TelemetrySourceBase<TestData>
{
    private readonly TestData[] _testData;

    public MockTelemetrySource(TestData[] testData)
    {
        _testData = testData;
    }

    public override async IAsyncEnumerable<TestData> ReadContinuousAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var data in _testData)
        {
            yield return data;
            await Task.Delay(10, ct);
        }
    }
}

// Usage
var testSource = new MockTelemetrySource(generateTestData())
    .RealtimeOnly();

await using var session = new GameSession()
    .AddSource(testSource)
    .OnData<TestData>(data => Assert.IsNotNull(data));
```

## More Examples

See:
- [TestApp/Program.cs](../GameTelemetry.TestApp/Program.cs) - Complete demo
- Game-specific READMEs in `GameTelemetry.Games.*` projects