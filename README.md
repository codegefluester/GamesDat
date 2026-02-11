# GamesDat

**High-performance telemetry capture framework for PC games on Windows**

GameTelemetry provides a simple, extensible API for capturing real-time game data from memory-mapped files, replay files, and network streams. Built for developers who need reliable, low-overhead telemetry collection with minimal code.

## Quick Start

```csharp
using GameTelemetry.Core;
using GameTelemetry.Games.ACC;

// Capture Assetto Corsa Competizione telemetry
await using var session = new GameSession()
    .AddSource(ACCSources.CreatePhysicsSource())
    .OnData<ACCPhysics>(data =>
        Console.WriteLine($"Speed: {data.SpeedKmh} km/h"));

await session.StartAsync();
```

**That's it.** Session data is automatically recorded to `./sessions/` with LZ4 compression.

## Features

- 🚀 **Minimal overhead** - <1% CPU impact at 100Hz capture rates
- 🎮 **Multi-source support** - Memory-mapped files, file watchers, network streams
- 💾 **Efficient storage** - Binary format with LZ4 compression (~15MB per hour)
- ⚡ **Real-time callbacks** - Process data as it arrives
- 🔌 **Extensible** - Add new games in minutes
- 🛡️ **Crash-resistant** - Graceful handling of incomplete sessions

## Supported Games

Out of the box:

| Game                       | Replay/Demo files         | Realtime data                   | Tested |
| -------------------------- | ------------------------- | ------------------------------- | ------ |
| Age of Empires IV          | ✅                        | ❌                              | ⏳     |
| Assetto Corsa Competizione | ❌                        | ✅ Physics, Graphics, Telemetry | ⏳     |
| Brawlhalla                 | ✅                        | ❌                              | ✅     |
| F1 25/24/23/22             | ⏳                        | ✅                              | ✅     |
| Rocket League              | ✅                        | ❌                              | ✅     |
| Rainbow Six Siege          | ✅                        | ❌                              | ✅     |
| Counter-Strike 2           | ✅                        | ❌                              | ✅     |
| DOTA 2                     | ✅                        | ❌                              | ⏳     |
| Fortnite                   | ✅                        | ❌                              | ⏳     |
| League of Legends          | ✅                        | ❌                              | ⏳     |
| Overwatch 2                | ✅                        | ❌                              | ⏳     |
| PUBG                       | ✅                        | ❌                              | ⏳     |
| Starcraft 2                | ✅                        | ❌                              | ⏳     |
| Tekken 8                   | ✅                        | ❌                              | ⏳     |
| Trackmania                 | ✅                        | ✅                              | ✅     |
| iRacing                    | ✅ IBT, Replay, OLAP/BLAP | ⏳ Telemetry and Session info   | ⏳     |
| Valorant                   | ✅                        | ❌                              | ✅     |

[Adding your own game →](docs/CREATING_SOURCES.md)

## Installation

```bash
# Clone repository
git clone https://github.com/yourusername/GameTelemetry.git

# Add project as a reference to your existing solution

```

NuGet packages coming soon.

## Architecture Overview

```
┌─────────────┐
│ GameSession │  ← Fluent API entry point
└──────┬──────┘
       │
   ┌───▼────────────────────────────┐
   │   ITelemetrySource<T>          │  ← Data sources
   ├────────────────────────────────┤
   │ • MemoryMappedFileSource<T>    │  Read from game memory
   │ • FileWatcherSource            │  Monitor replay folders
   │ • NetworkSource (future)       │  UDP/TCP streams
   └───┬────────────────────────────┘
       │
   ┌───▼──────────┐
   │ SessionWriter│  ← Binary output with LZ4
   └──────────────┘
```

**Core principles:**

- Sources produce typed data streams via `IAsyncEnumerable<T>`
- Optional per-source output with pluggable writers
- Real-time callbacks for live processing
- Zero-reflection hot path (generics only)

## Usage Examples

### Capture with auto-generated filename

```csharp
// Reads realtime physics from Assetto Corsa Competizione
await using var session = new GameSession()
    .AddSource(ACCSources.CreatePhysicsSource());

await session.StartAsync();
// → Saves to ./sessions/accphysics_20260125_120000.dat
```

### Multiple sources with custom paths

```csharp
await using var session = new GameSession()
    .AddSource(ACCSources.CreatePhysicsSource(), opt => opt
        .OutputTo("./my_race/physics.dat"))
    .AddSource(ACCSources.CreateGraphicsSource(), opt => opt
        .OutputTo("./my_race/graphics.dat"));

await session.StartAsync();
```

### Real-time processing only (no file output)

```csharp
await using var session = new GameSession()
    .AddSource(new TrackmaniaMemoryMappedSource(), opt => opt
        .RealtimeOnly())
    .OnData<TrackmaniaDataV3>(frame => {
        Console.WriteLine($"Speed: {frame.Speed} km/h");
    });

await session.StartAsync();
```

### Read and analyze sessions

```csharp
await foreach (var (timestamp, data) in SessionReader.ReadAsync<ACCPhysics>("session.dat"))
{
    Console.WriteLine($"{timestamp}: {data.SpeedKmh} km/h");
}
```

## Performance

Typical metrics (ACC physics at 100Hz):

- CPU overhead: **<1%**
- Memory: **~50MB**
- File size: **~15MB per hour** (compressed)
- Frame loss: **0%** with default settings

## Project Structure

```
GameTelemetry/
├── GameTelemetry.Core/          # Framework
│   ├── ITelemetrySource.cs
│   ├── GameSession.cs
│   ├── BinarySessionWriter.cs
│   └── SessionReader.cs
├── GameTelemetry.Games.ACC/     # ACC integration
├── GameTelemetry.Games.RocketLeague/
├── GameTelemetry.TestApp/       # Demo application
└── docs/                        # Documentation
```

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for:

- Code style guidelines
- PR process
- Adding new game integrations
- Performance optimization tips

## License

MIT License - see [LICENSE](LICENSE.md)

## Roadmap

- [ ] Network source (UDP/TCP) for F1 games, Counter-Strike game state etc.
- [ ] NuGet packages
- [ ] More game integrations (iRacing, F1, BeamNG)
- [ ] Session merging/splitting
- [ ] Cloud upload integrations (S3, Azure Blob)
- [ ] Real-time dashboard web UI

## Credits

Built with:

- [LZ4](https://github.com/MiloszKrajewski/K4os.Compression.LZ4) - Ultra-fast compression
- [Chart.js](https://www.chartjs.org/) - HTML visualizations

---

**Questions?** Open an issue or start a discussion.
