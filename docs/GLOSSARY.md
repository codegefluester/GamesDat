# GamesDat Glossary

Technical terms and concepts used in the GamesDat telemetry capture framework.

## Core Concepts

### Binary Session Writer

A writer implementation that serializes telemetry frames to a compressed binary file format. Uses LZ4 compression and stores frames as `[timestamp:long][size:int][data:byte[]]` sequences. Achieves approximately 3-4x compression ratios with minimal performance overhead.

### Game Session

The entry point for the fluent API and primary orchestrator for telemetry capture. Manages source lifecycle, routes data to writers and callbacks, and coordinates multiple telemetry sources running in parallel tasks.

### Memory-Mapped File (MMF)

A Windows shared memory region used for inter-process communication. Games expose real-time telemetry through named memory-mapped files that can be read by external applications. Accessed via `MemoryMappedFile.OpenExisting()` with zero-copy reads.

### Session Writer

An interface (`ISessionWriter`) for outputting telemetry data. Implementations handle how captured data is persisted, such as binary files, JSON, or custom formats. Writers are configured per-source rather than globally.

### Telemetry Source

An implementation of `ITelemetrySource<T>` that produces a stream of typed telemetry data. Sources abstract different data collection methods (memory-mapped files, file watchers, HTTP polling) into a unified interface using `IAsyncEnumerable<T>`.

## Data Structures

### Data Version

A version identifier (e.g., "1.0.0", "2.0.0") embedded in telemetry data structures to track breaking changes in field types or layouts. Version bumps indicate incompatible changes that may affect deserialization of old session files.

Example:
````csharp
// Version 2.0.0 changed Valid from int to bool
public struct StateData
{
    public bool Valid; // Breaking change from v1.0.0
}
````

### Struct Layout

The memory organization of data structures defined by `StructLayout` attributes. Specifies packing, alignment, and field ordering to match game's native memory layout. Critical for correct memory-mapped file reads.

### Unmanaged Type

A C# type constraint (`where T : unmanaged`) requiring types with no managed references. Unmanaged types can be safely copied as raw bytes, enabling zero-copy reads and binary serialization. Required for memory-mapped file sources and binary writers.

## Source Types

### File Watcher Source

A telemetry source that monitors directories for new or modified files using `FileSystemWatcher`. Commonly used for replay file analysis. Includes debouncing to prevent duplicate processing and tracks files already processed.

### HTTP Polling Source

A telemetry source that periodically fetches data from an HTTP endpoint. Uses configurable polling intervals, exponential backoff retry logic, and shared `HttpClient` instances. Base class `HttpPollingSourceBase<T>` provides reusable implementation for HTTP-based game APIs.

Example:
````csharp
// War Thunder /state endpoint at 60Hz
var source = WarThunderSources.CreateStateSource(hz: 60);
````

### Memory-Mapped File Source

A telemetry source (`MemoryMappedFileSource<T>`) that polls Windows shared memory regions. Provides low-overhead (~1-2 microseconds per read), high-frequency data capture from games that expose telemetry via MMF.

## Performance and Configuration

### Backpressure

Built-in flow control provided by `IAsyncEnumerable<T>` that prevents producers from overwhelming consumers. If the consumer (writer or callback) is slow, the source automatically slows its production rate.

### Debouncing

A delay mechanism that prevents rapid, repeated events from triggering multiple actions. Used in `FileWatcherSource` to avoid processing the same file multiple times when file system events fire repeatedly.

### Flush Interval

The number of frames between disk writes in `BinarySessionWriter`. Default is 10 frames, balancing data safety (maximum 10 frames lost on crash) with performance (reduced I/O overhead).

### LZ4 Compression

A fast, lossless compression algorithm used for session files. Provides 3-4x compression ratios with throughput of 500-1000 MB/s. Chosen over gzip for superior speed despite slightly larger file sizes.

### Polling Interval

The time delay between consecutive reads in polling-based sources. Specified as `TimeSpan` and determines capture frequency (e.g., `TimeSpan.FromMilliseconds(10)` for 100Hz). Should match the game's native update rate.

## Game-Specific Terms

### Indicators Endpoint

A War Thunder HTTP endpoint (`/indicators`) providing cockpit instrumentation data at lower frequency (typically 10Hz). Contains engine instruments, attitude indicators, and panel gauges. Returns data with string fields, incompatible with binary writers.

### State Endpoint

A War Thunder HTTP endpoint (`/state`) providing primary flight and vehicle telemetry at high frequency (typically 60Hz). Contains position, velocity, flight parameters, engine data, and control surface states. Returns numeric data suitable for binary serialization.

### Valid Field

A boolean field in telemetry structures indicating data validity. In War Thunder sources, `Valid = true` means the data is current and the player is actively in a match. `Valid = false` indicates no active session or stale data.

## API Patterns

### Exponential Backoff

A retry strategy that progressively increases delay between connection attempts: 1s → 2s → 4s → 8s → up to maximum delay. Used in `HttpPollingSource` to handle game not running or temporary disconnections without excessive polling.

### Factory Methods

Static convenience methods (e.g., `WarThunderSources.CreateStateSource()`) that create pre-configured source instances with sensible defaults. Encapsulates common configuration patterns and validates parameters.

### Fluent API

A method chaining pattern used by `GameSession` for readable configuration:
````csharp
await using var session = new GameSession()
    .AddSource(source)
    .OnData<T>(callback)
    .AutoOutput();
````

### Realtime-Only

A configuration mode (`.RealtimeOnly()`) that disables file output for a source. Data flows only to callbacks, useful for live dashboards or monitoring without disk I/O overhead.

## Error Handling

### Graceful Degradation

Design principle where the system continues operating despite errors. Examples: `SessionReader` treats incomplete frames as expected EOF rather than exceptions; sources retry connection failures with exponential backoff.

### Max Consecutive Errors

Configuration parameter limiting retry attempts before giving up. Default is 10 for HTTP sources. After exceeding this threshold, the source throws `InvalidOperationException` rather than retrying indefinitely.

## File Formats

### Session File

Binary file format (`.dat` extension) containing compressed telemetry frames. Format: sequence of `[timestamp:long][size:int][data:byte[]]` records. LZ4-compressed with typical size of ~15MB per hour at 100Hz capture rate.

### Timestamp

64-bit integer (`long`) representing `DateTime.Ticks`. Provides high-precision timing (100-nanosecond resolution) for telemetry frames. Used as the first field in session file records.

## Development Concepts

### Hot Path

Critical code paths executed frequently (e.g., per-frame serialization). Optimized to avoid allocations, use generics instead of reflection, and minimize computational overhead. Essential for maintaining <1% CPU impact.

### Zero-Copy Read

Memory access technique using `MemoryMappedViewAccessor` that reads data directly from shared memory without intermediate buffer allocation. Enables microsecond-level read performance for memory-mapped file sources.

## Breaking Changes

### Breaking Change

An incompatible modification to public APIs or data structures requiring version bumps. Examples include changing field types (int to bool), removing fields, or altering serialization formats. Documents should list breaking changes prominently.

Example from War Thunder v2.0.0:
- `Valid` field changed from `int` to `bool` in `StateData` and `IndicatorsData`
- DataVersion bumped from 1.0.0 to 2.0.0
- Old session files may not deserialize correctly
