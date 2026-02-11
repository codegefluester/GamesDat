using System;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    /// <summary>
    /// Abstract base class for F1 race replay file sources across different game years.
    /// Provides common functionality for monitoring F1 replay directories.
    /// </summary>
    public abstract class F1RaceReplaySourceBase : FileWatcherSourceBase
    {
        protected F1RaceReplaySourceBase(FileWatcherOptions options) : base(options)
        {
        }

        /// <summary>
        /// Build the full replay path for a specific F1 game year
        /// </summary>
        protected static string GetReplayPathForYear(string gameYear)
        {
            var documentsFolder = Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments,
                Environment.SpecialFolderOption.DoNotVerify);
            return System.IO.Path.Combine(documentsFolder, "My Games", $"F1 {gameYear}", "replays");
        }

        /// <summary>
        /// Create default FileWatcherOptions for F1 replay monitoring
        /// </summary>
        protected static FileWatcherOptions CreateDefaultOptions(string path)
        {
            return new FileWatcherOptions
            {
                Path = path,
                Patterns = new[] { "*.frr" },
                IncludeSubdirectories = false,
                DebounceDelay = TimeSpan.FromSeconds(2)
            };
        }

        /// <summary>
        /// Apply F1-specific defaults to options. Requires Path to be non-empty.
        /// </summary>
        protected static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                throw new ArgumentException("FileWatcherOptions.Path cannot be null or empty. Provide a path before calling ApplyDefaults.", nameof(options));
            }

            return new FileWatcherOptions
            {
                Path = options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? new[] { "*.frr" }
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories,
                DebounceDelay = options.DebounceDelay == default || options.DebounceDelay == TimeSpan.FromSeconds(1)
                    ? TimeSpan.FromSeconds(2)
                    : options.DebounceDelay
            };
        }
    }
}
