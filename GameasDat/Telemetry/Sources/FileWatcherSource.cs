using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameasDat.Core.Telemetry.Sources
{
    /// <summary>
    /// Monitors a directory for new or modified files matching specified patterns
    /// </summary>
    public class FileWatcherSource : TelemetrySourceBase<string>
    {
        private readonly string _path;
        private readonly string[] _patterns;
        private readonly bool _includeSubdirectories;
        private readonly TimeSpan _debounceDelay;

        private readonly ConcurrentDictionary<string, DateTime> _lastEventTime = new();
        private readonly HashSet<string> _processedFiles = new();
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Create a file watcher source
        /// </summary>
        /// <param name="path">Directory to monitor</param>
        /// <param name="patterns">File patterns to match (e.g., "*.replay", "*.json")</param>
        /// <param name="includeSubdirectories">Monitor subdirectories</param>
        /// <param name="debounceDelay">Minimum time between events for same file (prevents spam)</param>
        public FileWatcherSource(
            string path,
            string[] patterns,
            bool includeSubdirectories = false,
            TimeSpan? debounceDelay = null)
        {
            _path = path;
            _patterns = patterns;
            _includeSubdirectories = includeSubdirectories;
            _debounceDelay = debounceDelay ?? TimeSpan.FromSeconds(1);

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        /// <summary>
        /// Convenience constructor for single pattern
        /// </summary>
        public FileWatcherSource(
            string path,
            string pattern,
            bool includeSubdirectories = false,
            TimeSpan? debounceDelay = null)
            : this(path, new[] { pattern }, includeSubdirectories, debounceDelay)
        {
        }

        public override async IAsyncEnumerable<string> ReadContinuousAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var channel = System.Threading.Channels.Channel.CreateUnbounded<string>();

            // Setup file system watcher for each pattern
            var watchers = new List<FileSystemWatcher>();

            foreach (var pattern in _patterns)
            {
                var watcher = new FileSystemWatcher(_path, pattern)
                {
                    IncludeSubdirectories = _includeSubdirectories,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime
                };

                watcher.Created += (s, e) => OnFileEvent(e.FullPath, channel.Writer);
                watcher.Changed += (s, e) => OnFileEvent(e.FullPath, channel.Writer);

                watcher.EnableRaisingEvents = true;
                watchers.Add(watcher);
            }

            try
            {
                // Also scan for existing files on startup
                foreach (var pattern in _patterns)
                {
                    var existingFiles = Directory.GetFiles(_path, pattern,
                        _includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                    foreach (var file in existingFiles.OrderBy(f => File.GetCreationTime(f)))
                    {
                        if (!_processedFiles.Contains(file))
                        {
                            _processedFiles.Add(file);
                            await channel.Writer.WriteAsync(file, _cts.Token);
                        }
                    }
                }

                // Read from channel until cancelled
                await foreach (var filePath in channel.Reader.ReadAllAsync(_cts.Token))
                {
                    yield return filePath;
                }
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

        private void OnFileEvent(string filePath, System.Threading.Channels.ChannelWriter<string> writer)
        {
            // Debounce: only process if enough time has passed since last event
            var now = DateTime.UtcNow;
            if (_lastEventTime.TryGetValue(filePath, out var lastTime))
            {
                if (now - lastTime < _debounceDelay)
                    return; // Too soon, skip
            }

            _lastEventTime[filePath] = now;

            // Only emit each file once (prevents duplicate processing)
            if (!_processedFiles.Contains(filePath))
            {
                _processedFiles.Add(filePath);

                // Try to write to channel (non-blocking)
                writer.TryWrite(filePath);
            }
        }

        public override void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
