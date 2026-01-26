using System;

namespace GamesDat.Core.Telemetry.Sources
{
    /// <summary>
    /// Concrete file watcher source that monitors a directory for new or modified files
    /// </summary>
    public class FileWatcherSource : FileWatcherSourceBase
    {
        /// <summary>
        /// Create a file watcher source with options object
        /// </summary>
        /// <param name="options">Configuration options</param>
        public FileWatcherSource(FileWatcherOptions options)
            : base(options)
        {
        }

        /// <summary>
        /// Create a file watcher source with individual parameters
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
            : base(path, patterns, includeSubdirectories, debounceDelay)
        {
        }

        /// <summary>
        /// Convenience constructor for single pattern
        /// </summary>
        /// <param name="path">Directory to monitor</param>
        /// <param name="pattern">File pattern to match (e.g., "*.replay")</param>
        /// <param name="includeSubdirectories">Monitor subdirectories</param>
        /// <param name="debounceDelay">Minimum time between events for same file (prevents spam)</param>
        public FileWatcherSource(
            string path,
            string pattern,
            bool includeSubdirectories = false,
            TimeSpan? debounceDelay = null)
            : base(path, pattern, includeSubdirectories, debounceDelay)
        {
        }
    }
}
