using System;
using System.IO;

namespace GamesDat.Core.Telemetry.Sources.Trackmania
{
    /// <summary>
    /// File watcher source specifically configured for Trackmania replay files
    /// </summary>
    public class TrackmaniaReplayFileSource : FileWatcherSourceBase
    {
        /// <summary>
        /// Create a Trackmania replay file source with custom options
        /// </summary>
        /// <param name="options">Configuration options. If Path is not specified, uses default Trackmania replay location. If Patterns is not specified, uses "*.Replay.Gbx".</param>
        public TrackmaniaReplayFileSource(FileWatcherOptions options)
            : base(ApplyDefaults(options))
        {
        }

        /// <summary>
        /// Create a Trackmania replay file source
        /// </summary>
        /// <param name="customPath">Optional custom replay folder path. If null, uses default Trackmania replay location.</param>
        public TrackmaniaReplayFileSource(string? customPath = null)
            : base(
                path: customPath ?? GetDefaultReplayPath(),
                pattern: "*.Replay.Gbx",
                includeSubdirectories: true, // Trackmania organizes replays in subdirectories
                debounceDelay: TimeSpan.FromSeconds(2))
        {
        }

        private static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return new FileWatcherOptions
            {
                Path = string.IsNullOrEmpty(options.Path) ? GetDefaultReplayPath() : options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? new[] { "*.Replay.Gbx" }
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories || true, // Default to true for Trackmania
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(2)
                    : options.DebounceDelay
            };
        }

        /// <summary>
        /// Get the default Trackmania replay folder path
        /// Trackmania (2020) stores replays in: Documents\Trackmania\Replays
        /// Maniaplanet stores replays in: Documents\ManiaPlanet\Replays
        /// </summary>
        public static string GetDefaultReplayPath()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);

            // Try Trackmania (2020) first
            var tm2020Path = System.IO.Path.Combine(documents, "Trackmania", "Replays");
            if (Directory.Exists(tm2020Path))
            {
                return tm2020Path;
            }

            // Fallback to Maniaplanet
            var maniaPlanetPath = System.IO.Path.Combine(documents, "ManiaPlanet", "Replays");
            if (Directory.Exists(maniaPlanetPath))
            {
                return maniaPlanetPath;
            }

            // Return Trackmania 2020 path as default even if it doesn't exist
            return tm2020Path;
        }

        /// <summary>
        /// Validate that the Trackmania replay path exists and provide helpful error message
        /// </summary>
        protected override void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(
                    $"Trackmania replay folder not found: {path}\n" +
                    "Make sure Trackmania is installed and you've played at least one game.\n" +
                    "Expected locations:\n" +
                    "  Trackmania (2020): Documents\\Trackmania\\Replays\n" +
                    "  Maniaplanet: Documents\\ManiaPlanet\\Replays"); 
        }
    }
}
