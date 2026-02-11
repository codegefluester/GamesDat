# Refactor TelemetrySourceBase API from Pull to Callback-Based

## Context

The current `TelemetrySourceBase<T>` API forces all telemetry sources to implement `ReadContinuousAsync()` returning `IAsyncEnumerable<T>`. This creates a **pull-based model** where consumers iterate over data.

### The Problem

This pull model doesn't fit all source types naturally:

- **Memory-Mapped Sources** ✓ Natural fit - poll at intervals, yield data
- **UDP Sources** ✗ Awkward - inherently push-based (packets arrive asynchronously), but wrapped in `IAsyncEnumerable` using blocking `ReceiveAsync()`
- **File Watcher Sources** ✗ Awkward - inherently push-based (file system events), but converted to pull using `Channel<T>` adapters

The result is unnecessary complexity: push sources are forced through adapters to convert events → pull semantics → callbacks (in GameSession).

### Key Insight

**All telemetry sources are fundamentally push-based at the consumer level.** GameSession wants to be notified when data arrives, regardless of whether that data came from polling, network packets, or file system events. The current abstraction fights this natural flow.

## Recommended Approach: Callback-Based API

Replace `IAsyncEnumerable<T> ReadContinuousAsync(CancellationToken)` with `Task RunAsync(Action<T> onData, CancellationToken)`.

### Benefits

1. **Natural for push sources** - UDP and file watchers can directly invoke the callback
2. **Works fine for pull sources** - Memory-mapped just calls the callback in their polling loop
3. **Removes adapters** - No more Channel in FileWatcher, no awkward ReceiveAsync wrapping
4. **Simpler GameSession** - Single unified runner instead of different patterns
5. **Clear data flow** - `RunAsync(onData, ct)` makes it explicit that data flows through the callback

## Implementation Plan

### 1. Update `ITelemetrySource<T>` Interface

**File:** `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\ITelemetrySource.cs`

**Changes:**
- Replace `IAsyncEnumerable<T> ReadContinuousAsync(CancellationToken ct = default)`
- With `Task RunAsync(Action<T> onData, CancellationToken ct = default)`

**Before:**
```csharp
public interface ITelemetrySource<T> : IDisposable
{
    string? OutputPath { get; set; }
    ISessionWriter? Writer { get; set; }

    IAsyncEnumerable<T> ReadContinuousAsync(CancellationToken ct = default);

    ITelemetrySource<T> OutputTo(string path);
    ITelemetrySource<T> UseWriter(ISessionWriter writer);
    ITelemetrySource<T> RealtimeOnly();
    ITelemetrySource<T> AutoOutput();
}
```

**After:**
```csharp
public interface ITelemetrySource<T> : IDisposable
{
    string? OutputPath { get; set; }
    ISessionWriter? Writer { get; set; }

    /// <summary>
    /// Start the telemetry source and invoke onData callback for each data item.
    /// This method should block until cancellation is requested.
    /// </summary>
    Task RunAsync(Action<T> onData, CancellationToken ct = default);

    ITelemetrySource<T> OutputTo(string path);
    ITelemetrySource<T> UseWriter(ISessionWriter writer);
    ITelemetrySource<T> RealtimeOnly();
    ITelemetrySource<T> AutoOutput();
}
```

### 2. Update `TelemetrySourceBase<T>` Base Class

**File:** `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\TelemetrySourceBase.cs`

**Changes:**
- Update abstract method signature from `ReadContinuousAsync` to `RunAsync`
- Keep all fluent API methods unchanged

**Before:**
```csharp
public abstract IAsyncEnumerable<T> ReadContinuousAsync(CancellationToken ct = default);
```

**After:**
```csharp
public abstract Task RunAsync(Action<T> onData, CancellationToken ct = default);
```

### 3. Update `UdpSourceBase<T>` - Remove IAsyncEnumerable Wrapper

**File:** `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\Sources\UdpSourceBase.cs`

**Changes:**
- Change method from `IAsyncEnumerable<T> ReadContinuousAsync()` to `Task RunAsync(Action<T> onData, ...)`
- Remove `yield return`, directly invoke callback

**Before:**
```csharp
public override async IAsyncEnumerable<T> ReadContinuousAsync([EnumeratorCancellation] CancellationToken ct = default)
{
    _isListening = true;
    try
    {
        while (!ct.IsCancellationRequested)
        {
            var result = await _listener.ReceiveAsync(ct);
            var data = result.Buffer;
            foreach (var item in ProcessData(data))
            {
                yield return item;  // Awkward wrapper
            }
        }
    }
    finally
    {
        _isListening = false;
        _listener.Dispose();
    }
}
```

**After:**
```csharp
public override async Task RunAsync(Action<T> onData, CancellationToken ct = default)
{
    _isListening = true;
    try
    {
        while (!ct.IsCancellationRequested)
        {
            var result = await _listener.ReceiveAsync(ct);
            var data = result.Buffer;
            foreach (var item in ProcessData(data))
            {
                onData(item);  // Direct callback - clean!
            }
        }
    }
    finally
    {
        _isListening = false;
        _listener.Dispose();
    }
}
```

### 4. Update `FileWatcherSourceBase` - Remove Channel Adapter

**File:** `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\Sources\FileWatcherSourceBase.cs`

**Changes:**
- Change method from `IAsyncEnumerable<string> ReadContinuousAsync()` to `Task RunAsync(Action<string> onData, ...)`
- Remove Channel adapter
- Directly invoke callback from event handlers
- Use `Task.Delay(Timeout.Infinite, ct)` to keep alive until cancellation

**Before (lines 157-213):**
```csharp
public override async IAsyncEnumerable<string> ReadContinuousAsync(
    [EnumeratorCancellation] CancellationToken ct = default)
{
    _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    var channel = System.Threading.Channels.Channel.CreateUnbounded<string>();  // Adapter

    var watchers = new List<FileSystemWatcher>();

    foreach (var pattern in Patterns)
    {
        var watcher = new FileSystemWatcher(Path, pattern) { ... };
        watcher.Created += (s, e) => OnFileEvent(e.FullPath, channel.Writer);  // Writes to channel
        watcher.Changed += (s, e) => OnFileEvent(e.FullPath, channel.Writer);
        watcher.EnableRaisingEvents = true;
        watchers.Add(watcher);
    }

    try
    {
        // Scan existing files
        foreach (var pattern in Patterns) { ... }

        // Read from channel - pull from push
        await foreach (var filePath in channel.Reader.ReadAllAsync(_cts.Token))
        {
            yield return filePath;
        }
    }
    finally { ... }
}

private void OnFileEvent(string filePath, ChannelWriter<string> writer)
{
    // Debounce logic...
    writer.TryWrite(filePath);  // Write to channel
}
```

**After:**
```csharp
public override async Task RunAsync(Action<string> onData, CancellationToken ct = default)
{
    _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    var watchers = new List<FileSystemWatcher>();

    foreach (var pattern in Patterns)
    {
        var watcher = new FileSystemWatcher(Path, pattern)
        {
            IncludeSubdirectories = IncludeSubdirectories,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
        };

        // Direct callback from event handler
        watcher.Created += (s, e) => OnFileEvent(e.FullPath, onData);
        watcher.Changed += (s, e) => OnFileEvent(e.FullPath, onData);

        watcher.EnableRaisingEvents = true;
        watchers.Add(watcher);
    }

    try
    {
        // Scan existing files on startup
        foreach (var pattern in Patterns)
        {
            var existingFiles = Directory.GetFiles(Path, pattern,
                IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (var file in existingFiles.OrderBy(f => File.GetCreationTime(f)))
            {
                if (!_processedFiles.Contains(file) && ShouldProcessFile(file))
                {
                    _processedFiles.Add(file);
                    onData(file);  // Direct callback
                }
            }
        }

        // Keep alive until cancelled
        await Task.Delay(Timeout.Infinite, _cts.Token);
    }
    catch (TaskCanceledException)
    {
        // Normal cancellation
    }
    finally
    {
        foreach (var watcher in watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
    }
}

private void OnFileEvent(string filePath, Action<string> onData)
{
    // Debounce logic
    var now = DateTime.UtcNow;
    if (_lastEventTime.TryGetValue(filePath, out var lastTime))
    {
        if (now - lastTime < DebounceDelay)
            return;
    }

    _lastEventTime[filePath] = now;

    // Only emit each file once and check custom filter
    if (!_processedFiles.Contains(filePath) && ShouldProcessFile(filePath))
    {
        _processedFiles.Add(filePath);
        onData(filePath);  // Direct callback
    }
}
```

### 5. Update `MemoryMappedFileSource<T>` - Convert Yield to Callback

**File:** `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\Sources\MemoryMappedFileSource.cs`

**Changes:**
- Change from `IAsyncEnumerable<T>` to `Task`
- Replace `yield return` with direct callback invocation

**Before:**
```csharp
public override async IAsyncEnumerable<T> ReadContinuousAsync([EnumeratorCancellation] CancellationToken ct = default)
{
    _mmf = MemoryMappedFile.OpenExisting(_mapName, MemoryMappedFileRights.Read);
    _accessor = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

    try
    {
        while (!ct.IsCancellationRequested)
        {
            T data;
            _accessor.Read(0, out data);
            yield return data;
            await Task.Delay(_pollInterval, ct);
        }
    }
    finally
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
    }
}
```

**After:**
```csharp
public override async Task RunAsync(Action<T> onData, CancellationToken ct = default)
{
    _mmf = MemoryMappedFile.OpenExisting(_mapName, MemoryMappedFileRights.Read);
    _accessor = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

    try
    {
        while (!ct.IsCancellationRequested)
        {
            T data;
            _accessor.Read(0, out data);
            onData(data);  // Callback instead of yield
            await Task.Delay(_pollInterval, ct);
        }
    }
    finally
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
    }
}
```

### 6. Update All Other Memory-Mapped Sources

Apply same changes to:
- `TrackmaniaMemoryMappedFileSource.cs`
- Any ACC sources (Physics, Graphics, Static)
- `ACCCombinedSource.cs` - Update to use `RunAsync` when consuming child sources

### 7. Update `GameSession.SourceRunner<T>` Consumer

**File:** `C:\Users\maste\source\repos\GamesDat\GamesDat\GameSession.cs`

**Changes:**
- Replace `await foreach` loop with `RunAsync()` callback
- Simplify runner logic

**Before (lines 140-177):**
```csharp
private async Task RunAsync(CancellationToken ct)
{
    try
    {
        Console.WriteLine($"[{SourceTypeName}] Starting source...");
        int frameCount = 0;

        await foreach (var data in _source.ReadContinuousAsync(ct))  // Pull model
        {
            frameCount++;

            if (frameCount <= 3)
            {
                Console.WriteLine($"[{SourceTypeName}] Frame {frameCount} received");
            }

            // Write to file if writer configured
            if (_writer != null)
            {
                _writer.WriteFrame(data, DateTime.UtcNow.Ticks);
            }

            // Invoke callbacks
            if (_callbacks.TryGetValue(_dataType, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    try
                    {
                        callback.DynamicInvoke(data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{SourceTypeName}] ERROR in callback: {ex.Message}");
                    }
                }
            }
        }

        Console.WriteLine($"[{SourceTypeName}] Source ended after {frameCount} frames");
    }
    catch (Exception ex) { ... }
}
```

**After:**
```csharp
private async Task RunAsync(CancellationToken ct)
{
    try
    {
        Console.WriteLine($"[{SourceTypeName}] Starting source...");
        int frameCount = 0;

        await _source.RunAsync(data =>  // Callback model - simpler!
        {
            frameCount++;

            if (frameCount <= 3)
            {
                Console.WriteLine($"[{SourceTypeName}] Frame {frameCount} received");
            }

            // Write to file if writer configured
            if (_writer != null)
            {
                _writer.WriteFrame(data, DateTime.UtcNow.Ticks);
            }

            // Invoke callbacks
            if (_callbacks.TryGetValue(_dataType, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    try
                    {
                        callback.DynamicInvoke(data);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[{SourceTypeName}] ERROR in callback: {ex.Message}");
                    }
                }
            }
        }, ct);

        Console.WriteLine($"[{SourceTypeName}] Source ended after {frameCount} frames");
    }
    catch (Exception ex) { ... }
}
```

## Critical Files to Modify

1. ✓ `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\ITelemetrySource.cs` - Interface signature
2. ✓ `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\TelemetrySourceBase.cs` - Abstract base class
3. ✓ `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\Sources\UdpSourceBase.cs` - Remove IAsyncEnumerable wrapper
4. ✓ `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\Sources\FileWatcherSourceBase.cs` - Remove Channel adapter
5. ✓ `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\Sources\MemoryMappedFileSource.cs` - Convert yield to callback
6. ✓ `C:\Users\maste\source\repos\GamesDat\GamesDat\GameSession.cs` - Update SourceRunner consumer
7. ⚠️ All game-specific implementations inheriting from these bases (should automatically work)
8. ⚠️ `TrackmaniaMemoryMappedFileSource.cs` - If it overrides ReadContinuousAsync
9. ⚠️ `ACCCombinedSource.cs` - Update to use RunAsync for child sources
10. ⚠️ Any tests using `ReadContinuousAsync` directly

## Verification Plan

### 1. Compilation Check
```bash
dotnet build
```
- Ensure all sources compile without errors
- Check for any missed implementations

### 2. Unit Tests
Run existing test suites:
```bash
dotnet test
```
- FileWatcherSourceTests should still pass (behavior unchanged, just different API)
- Any integration tests using sources should be updated

### 3. Manual Testing

**Test UDP Source (F1):**
1. Start F1 game with telemetry enabled
2. Run F1 demo application
3. Verify packets are received and logged
4. Check output file is written correctly

**Test File Watcher (Trackmania Replays):**
1. Configure TrackmaniaReplayFileSource
2. Save a replay in Trackmania
3. Verify file path is detected and callback fires
4. Check debouncing works (no duplicates)

**Test Memory-Mapped (ACC):**
1. Start Assetto Corsa Competizione
2. Run ACC demo application
3. Verify telemetry data streams correctly
4. Check combined source aggregates Physics/Graphics/Static properly

### 4. WPF Demo Applications
- Launch Demo.Wpf application
- Test F1RealtimeSourceViewModel
- Test FileWatcherSourceViewModel
- Verify UI updates correctly with new API

### 5. Behavior Verification Checklist
- [ ] UDP sources still receive packets correctly
- [ ] File watchers still detect new files
- [ ] Memory-mapped sources still poll at correct intervals
- [ ] Debouncing in file watchers still works
- [ ] Deduplication (processed files) still prevents duplicates
- [ ] GameSession callbacks (OnData<T>) still fire
- [ ] File writing still works with configured writers
- [ ] Fluent API (OutputTo, RealtimeOnly, etc.) still works
- [ ] Disposal/cleanup still happens properly

## Additional Considerations

### Optional: IAsyncEnumerable Extension Method

If needed for backward compatibility or alternative consumption, add an extension method:

**File:** Create `C:\Users\maste\source\repos\GamesDat\GamesDat\Telemetry\TelemetrySourceExtensions.cs`

```csharp
public static class TelemetrySourceExtensions
{
    public static async IAsyncEnumerable<T> ReadContinuousAsync<T>(
        this ITelemetrySource<T> source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var channel = Channel.CreateUnbounded<T>();

        var runTask = source.RunAsync(data => channel.Writer.TryWrite(data), ct);

        await foreach (var item in channel.Reader.ReadAllAsync(ct))
        {
            yield return item;
        }

        await runTask;
    }
}
```

This allows consumers who prefer pull semantics to still use `await foreach`.

## Summary

This refactoring removes the awkward pull-model abstraction and embraces the natural push-based flow of telemetry data. Each source type now uses its most natural mechanism:
- UDP sources directly invoke callbacks when packets arrive
- File watchers directly invoke callbacks when files are detected
- Memory-mapped sources invoke callbacks in their polling loop

The result is simpler, cleaner code with no unnecessary adapters, while maintaining the same fluent configuration API and consumer behavior.
