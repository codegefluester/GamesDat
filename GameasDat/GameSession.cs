using GameasDat.Core.Telemetry;
using GameasDat.Core.Writer;
using System.Collections.Concurrent;

namespace GameasDat.Core
{
    /// <summary>
    /// Main entry point for GameTelemetry. Fluent API for capturing game data.
    /// </summary>
    public class GameSession : IAsyncDisposable
    {
        private readonly List<ISourceRunner> _sourceRunners = new();
        private readonly ConcurrentDictionary<Type, List<Delegate>> _realtimeCallbacks = new();
        private CancellationTokenSource? _cts;
        private readonly string _defaultOutputDirectory;

        public GameSession(string? defaultOutputDirectory = null)
        {
            _defaultOutputDirectory = defaultOutputDirectory ?? "./sessions";
        }

        public GameSession AddSource<T>(
            ITelemetrySource<T> source,
            Action<SourceOptions>? configure = null) where T : unmanaged
        {
            var sourceType = source.GetType();

            if (_sourceRunners.Any(s => s.SourceTypeName == sourceType.Name))
                throw new InvalidOperationException(
                    $"Source of type {sourceType.Name} already added. Only one source per type is allowed.");

            var options = new SourceOptions();
            configure?.Invoke(options);

            var outputPath = options.OutputPath;
            if (outputPath == "auto")
            {
                Directory.CreateDirectory(_defaultOutputDirectory);
                var sourceName = sourceType.Name.Replace("Source", "").ToLowerInvariant();
                outputPath = Path.Combine(_defaultOutputDirectory,
                    $"{sourceName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.dat");
            }

            ISessionWriter? sessionWriter = null;
            if (outputPath != null)
            {
                sessionWriter = options.Writer ?? new BinarySessionWriter();
                sessionWriter.Start(outputPath);
                Console.WriteLine($"[{sourceType.Name}] Recording to: {outputPath}");
            }
            else
            {
                Console.WriteLine($"[{sourceType.Name}] Real-time only (no output file)");
            }

            // Create a generic runner that knows how to handle this specific source type
            var runner = new SourceRunner<T>(source, sessionWriter, typeof(T), sourceType.Name, _realtimeCallbacks);
            _sourceRunners.Add(runner);

            return this;
        }

        public GameSession OnData<T>(Action<T> callback)
        {
            var dataType = typeof(T);

            if (!_realtimeCallbacks.ContainsKey(dataType))
                _realtimeCallbacks[dataType] = new List<Delegate>();

            _realtimeCallbacks[dataType].Add(callback);
            return this;
        }

        public Task<GameSession> StartAsync(CancellationToken ct = default)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            foreach (var runner in _sourceRunners)
            {
                runner.Start(_cts.Token);
            }

            Console.WriteLine($"\nStarted {_sourceRunners.Count} source(s). Press Ctrl+C to stop.\n");
            return Task.FromResult(this);
        }

        public async Task StopAsync()
        {
            Console.WriteLine("\nStopping session...");

            _cts?.Cancel();

            foreach (var runner in _sourceRunners)
            {
                await runner.StopAsync();
            }

            Console.WriteLine("Session stopped");
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
            _cts?.Dispose();
        }

        // Interface for source runners
        private interface ISourceRunner
        {
            string SourceTypeName { get; }
            void Start(CancellationToken ct);
            Task StopAsync();
        }

        // Generic runner that handles a specific source type
        private class SourceRunner<T> : ISourceRunner where T : unmanaged
        {
            private readonly ITelemetrySource<T> _source;
            private readonly ISessionWriter? _writer;
            private readonly Type _dataType;
            private readonly ConcurrentDictionary<Type, List<Delegate>> _callbacks;
            private Task? _runningTask;

            public string SourceTypeName { get; }

            public SourceRunner(
                ITelemetrySource<T> source,
                ISessionWriter? writer,
                Type dataType,
                string sourceTypeName,
                ConcurrentDictionary<Type, List<Delegate>> callbacks)
            {
                _source = source;
                _writer = writer;
                _dataType = dataType;
                SourceTypeName = sourceTypeName;
                _callbacks = callbacks;
            }

            public void Start(CancellationToken ct)
            {
                _runningTask = RunAsync(ct);
            }

            private async Task RunAsync(CancellationToken ct)
            {
                try
                {
                    Console.WriteLine($"[{SourceTypeName}] Starting source...");
                    int frameCount = 0;

                    await foreach (var data in _source.ReadContinuousAsync(ct))
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
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine($"[{SourceTypeName}] ERROR: Memory-mapped file not found - is the game running?");
                    Console.WriteLine($"  Details: {ex.Message}");
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{SourceTypeName}] ERROR: {ex.GetType().Name}: {ex.Message}");
                    Console.WriteLine($"  Stack: {ex.StackTrace}");
                }
            }

            public async Task StopAsync()
            {
                _writer?.Stop();
                _writer?.Dispose();

                if (_source is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                if (_runningTask != null)
                {
                    await _runningTask;
                }
            }
        }
    }
}
