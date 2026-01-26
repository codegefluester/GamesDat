using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GamesDat.Core.Telemetry.Sources
{
    /// <summary>
    /// Abstract base class for file watcher sources that monitor directories for new or modified files
    /// </summary>
    public abstract class FileWatcherSourceBase : TelemetrySourceBase<string>
    {
        private readonly ConcurrentDictionary<string, DateTime> _lastEventTime = new();
        private readonly HashSet<string> _processedFiles = new();
        private CancellationTokenSource? _cts;

        /// <summary>
        /// Directory path to monitor
        /// </summary>
        protected string Path { get; set; }

        /// <summary>
        /// File patterns to match (e.g., "*.replay", "*.json")
        /// </summary>
        protected string[] Patterns { get; set; }

        /// <summary>
        /// Whether to monitor subdirectories recursively
        /// </summary>
        protected bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// Minimum time between events for the same file to prevent duplicate notifications
        /// </summary>
        protected TimeSpan DebounceDelay { get; set; }

        /// <summary>
        /// Create a file watcher source with options object
        /// </summary>
        /// <param name="options">Configuration options</param>
        protected FileWatcherSourceBase(FileWatcherOptions options)
        {
            Path = options.Path;
            Patterns = options.Patterns;
            IncludeSubdirectories = options.IncludeSubdirectories;
            DebounceDelay = options.DebounceDelay;

            ValidatePath(Path);
        }

        /// <summary>
        /// Create a file watcher source with individual parameters
        /// </summary>
        /// <param name="path">Directory to monitor</param>
        /// <param name="patterns">File patterns to match (e.g., "*.replay", "*.json")</param>
        /// <param name="includeSubdirectories">Monitor subdirectories</param>
        /// <param name="debounceDelay">Minimum time between events for same file (prevents spam)</param>
        protected FileWatcherSourceBase(
            string path,
            string[] patterns,
            bool includeSubdirectories = false,
            TimeSpan? debounceDelay = null)
        {
            Path = path;
            Patterns = patterns;
            IncludeSubdirectories = includeSubdirectories;
            DebounceDelay = debounceDelay ?? TimeSpan.FromSeconds(1);

            ValidatePath(path);
        }

        /// <summary>
        /// Convenience constructor for single pattern
        /// </summary>
        /// <param name="path">Directory to monitor</param>
        /// <param name="pattern">File pattern to match (e.g., "*.replay")</param>
        /// <param name="includeSubdirectories">Monitor subdirectories</param>
        /// <param name="debounceDelay">Minimum time between events for same file (prevents spam)</param>
        protected FileWatcherSourceBase(
            string path,
            string pattern,
            bool includeSubdirectories = false,
            TimeSpan? debounceDelay = null)
            : this(path, new[] { pattern }, includeSubdirectories, debounceDelay)
        {
        }

        /// <summary>
        /// Change the directory path to watch
        /// </summary>
        public ITelemetrySource<string> WatchPath(string path)
        {
            ValidatePath(path);
            Path = path;
            return this;
        }

        /// <summary>
        /// Set a single file pattern to match
        /// </summary>
        public ITelemetrySource<string> WithPattern(string pattern)
        {
            Patterns = new[] { pattern };
            return this;
        }

        /// <summary>
        /// Set multiple file patterns to match
        /// </summary>
        public ITelemetrySource<string> WithPatterns(params string[] patterns)
        {
            Patterns = patterns;
            return this;
        }

        /// <summary>
        /// Enable or disable recursive subdirectory monitoring
        /// </summary>
        public ITelemetrySource<string> WithSubdirectories(bool include = true)
        {
            IncludeSubdirectories = include;
            return this;
        }

        /// <summary>
        /// Configure the debounce delay for file events
        /// </summary>
        public ITelemetrySource<string> WithDebounceDelay(TimeSpan delay)
        {
            DebounceDelay = delay;
            return this;
        }

        /// <summary>
        /// Validate the directory path. Override to add custom validation logic.
        /// </summary>
        /// <param name="path">Path to validate</param>
        protected virtual void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        /// <summary>
        /// Determine whether a file should be processed. Override to add custom filtering logic.
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if the file should be processed, false otherwise</returns>
        protected virtual bool ShouldProcessFile(string filePath)
        {
            return true;
        }

        public override async IAsyncEnumerable<string> ReadContinuousAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var channel = System.Threading.Channels.Channel.CreateUnbounded<string>();

            // Setup file system watcher for each pattern
            var watchers = new List<FileSystemWatcher>();

            foreach (var pattern in Patterns)
            {
                var watcher = new FileSystemWatcher(Path, pattern)
                {
                    IncludeSubdirectories = IncludeSubdirectories,
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
                foreach (var pattern in Patterns)
                {
                    var existingFiles = Directory.GetFiles(Path, pattern,
                        IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                    foreach (var file in existingFiles.OrderBy(f => File.GetCreationTime(f)))
                    {
                        if (!_processedFiles.Contains(file) && ShouldProcessFile(file))
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
                if (now - lastTime < DebounceDelay)
                    return; // Too soon, skip
            }

            _lastEventTime[filePath] = now;

            // Only emit each file once (prevents duplicate processing) and check custom filter
            if (!_processedFiles.Contains(filePath) && ShouldProcessFile(filePath))
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
