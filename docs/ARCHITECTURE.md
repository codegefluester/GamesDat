# Architecture Deep Dive

This document explains the internal design of GameTelemetry.

## Design Goals

1. **Minimal Overhead** - <1% CPU even at 100Hz
2. **Simple API** - Get started in 3 lines of code
3. **Extensible** - Add new games without modifying core
4. **Type-Safe** - Compile-time checking, no runtime surprises
5. **Crash-Resistant** - Handle incomplete sessions gracefully

## Core Components

### GameSession

Entry point for the fluent API. Manages source lifecycle and output.

**Responsibilities:**

- Register sources and configure outputs
- Start/stop background tasks
- Route data to writers and callbacks

**Design decisions:**

- Uses generics to avoid reflection in hot path
- Each source runs in its own `Task`
- Per-source output writers (not global)

### ITelemetrySource<T>

Interface for all data sources.

```csharp
public interface ITelemetrySource<T> : IDisposable
{
    IAsyncEnumerable<T> ReadContinuousAsync(CancellationToken ct = default);
}
```

**Why `IAsyncEnumerable`?**

- Natural streaming abstraction
- Backpressure handling built-in
- Composable with LINQ
- Cancellation support

**Type constraint:** Generic `T` has no constraint at interface level. Constraints applied at usage (e.g., `where T : unmanaged` for binary writer).

### Source Implementations

#### MemoryMappedFileSource<T>

Polls a Windows shared memory region.

**Key details:**

- Uses `MemoryMappedFile.OpenExisting()`
- Polling loop with `Task.Delay()`
- Zero-copy reads via `MemoryMappedViewAccessor`
- Requires `T : unmanaged`

**Performance:**

- Read: ~1-2 microseconds
- CPU overhead: ~0.5% at 100Hz

#### FileWatcherSource

Monitors directory for new/modified files.

**Key details:**

- Uses `FileSystemWatcher` per pattern
- Debouncing to prevent spam
- Tracks processed files (no duplicates)
- Scans existing files on startup

**Edge cases handled:**

- Rapid file modifications (debouncing)
- Subdirectory recursion (optional)
- File system events firing multiple times

### BinarySessionWriter

Writes telemetry frames to compressed binary file.

**Format:**

```
[timestamp:long][size:int][data:byte[]]
[timestamp:long][size:int][data:byte[]]
...
```

**Compression:**

- LZ4 via K4os.Compression.LZ4.Streams
- ~3-4x compression ratio for telemetry
- ~500-1000 MB/s throughput

**Flushing strategy:**

- Every 10 frames by default
- Balances data safety vs. performance
- Loses at most 10 frames on crash

**Design decisions:**

- Size field enables validation on read
- Timestamp in ticks (8 bytes, high precision)
- Lock protects writes (sources may be parallel)

### SessionReader

Reads back telemetry sessions.

**Key features:**

- Streaming (doesn't load entire file)
- Handles incomplete frames (from Ctrl+C)
- LZ4 decompression errors treated as EOF
- Validates struct size on read

**Error handling:**

- Partial timestamp → Warning, stop gracefully
- Partial size field → Warning, stop gracefully
- Partial data → Warning, stop gracefully
- LZ4 corruption → Warning, stop gracefully

No exceptions thrown for expected "session interrupted" scenarios.

## Data Flow

```
┌─────────────────────────────────────────────┐
│ MemoryMappedFileSource<ACCPhysics>         │
│   Poll every 10ms                           │
└────────────┬────────────────────────────────┘
             │ IAsyncEnumerable<ACCPhysics>
             │
             ▼
    ┌────────────────────┐
    │   SourceRunner<T>  │ ← One per source
    │   (runs in Task)   │
    └────┬───────────┬───┘
         │           │
         │           └──────────────────┐
         │                              │
         ▼                              ▼
┌────────────────────┐      ┌──────────────────────┐
│ BinarySessionWriter│      │ OnData<T> Callbacks  │
│  WriteFrame()      │      │  User code           │
└────────────────────┘      └──────────────────────┘
```

**Threading:**

- Each source has its own `Task`
- Writer methods are synchronized (lock)
- Callbacks invoked sequentially per source
- No cross-source coordination needed

## Performance Characteristics

### Memory Usage

Typical session (1 source):

- Base overhead: ~5 MB
- Per source: ~10 MB (buffers, state)
- File buffer: ~4 KB
- LZ4 buffer: ~64 KB

**Total: ~50 MB** for single-source capture

### CPU Usage

At 100Hz (10ms poll interval):

- Read MMF: ~1 μs
- Serialize: ~2 μs
- Compress: ~5 μs
- Write: ~1 μs

**Total: ~9 μs per frame = 0.09% CPU**

Overhead measured: ~0.5-1% (includes Task scheduling, GC, etc.)

### Disk I/O

With 10-frame flush interval:

- Write every 100ms
- Batch size: ~1-2 KB compressed
- Sequential writes (HDD-friendly)

**Impact:** Negligible on SSD, <1ms latency on HDD

### File Size

Example: ACC physics at 100Hz

- Raw struct: ~350 bytes
- Compressed: ~80 bytes (4.4x ratio)
- Per hour: **28.8 MB**

## Extension Points

### Custom Writers

Implement `ISessionWriter`:

```csharp
public class JsonSessionWriter : ISessionWriter
{
    public void Start(string filePath) { }
    public void WriteFrame<T>(T data, long timestamp) where T : unmanaged { }
    public void Stop() { }
    public void Dispose() { }
}
```

Use with:

```csharp
.AddSource(source, opt => opt.UseWriter(new JsonSessionWriter()))
```

### Custom Sources

Implement `ITelemetrySource<T>`:

```csharp
public class UdpTelemetrySource : ITelemetrySource<MyData>
{
    public async IAsyncEnumerable<MyData> ReadContinuousAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var client = new UdpClient(port);
        while (!ct.IsCancellationRequested)
        {
            var result = await client.ReceiveAsync();
            yield return ParseData(result.Buffer);
        }
    }

    public void Dispose() { }
}
```

## Future Architecture

### Planned Features

**Network Source**

- UDP/TCP telemetry
- Configurable packet format
- Automatic reassembly

**Session Processors**

- Background processing pipeline
- Lap splitting, sector detection
- Data enrichment

**Cloud Uploaders**

- S3, Azure Blob, Google Cloud
- Incremental upload
- Retry logic

### Not Planned

**Multi-process coordination** - Keep it simple, one process per capture
**Real-time streaming** - Use callbacks, not WebSockets/SignalR
**Database integration** - Export to CSV, load in your DB

## Trade-offs

### Decisions We Made

| Decision             | Why            | Trade-off             |
| -------------------- | -------------- | --------------------- |
| Binary format        | Performance    | Less human-readable   |
| Per-source outputs   | Flexibility    | More files            |
| Polling (not events) | Simplicity     | Slight latency        |
| LZ4 (not gzip)       | Speed          | Slightly larger files |
| No async writes      | Predictability | Small blocking        |
| Struct constraints   | Type safety    | Some flexibility lost |

### Alternative Designs Considered

**Global output file** ❌

- Complex to read back (mixed types)
- Doesn't scale to many sources

**Event-based MMF** ❌

- Windows API complexity
- Polling is "good enough"

**JSON format** ❌

- 50x slower to write
- 5x larger files
- Not worth human readability

**MessagePack** ❌

- Complexity with unsafe structs
- LZ4 + binary is simpler and faster

## Questions?

Open a discussion or issue for architecture questions.
