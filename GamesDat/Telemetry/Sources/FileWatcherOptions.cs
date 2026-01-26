using System;

namespace GamesDat.Core.Telemetry.Sources
{
    /// <summary>
    /// Configuration options for file watcher sources
    /// </summary>
    public class FileWatcherOptions
    {
        /// <summary>
        /// Directory path to monitor for file changes
        /// </summary>
        public string Path { get; set; } = "";

        /// <summary>
        /// File patterns to match (e.g., "*.replay", "*.json")
        /// </summary>
        public string[] Patterns { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Whether to monitor subdirectories recursively
        /// </summary>
        public bool IncludeSubdirectories { get; set; } = false;

        /// <summary>
        /// Minimum time between events for the same file to prevent duplicate notifications
        /// </summary>
        public TimeSpan DebounceDelay { get; set; } = TimeSpan.FromSeconds(1);
    }
}
