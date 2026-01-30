using System;
using System.IO;

namespace GamesDat.Core.Telemetry.Sources.LeagueOfLegends
{
    /// <summary>
    /// File watcher source specifically configured for League of Legends replay files
    /// </summary>
    public class LeagueOfLegendsReplayFileSource : FileWatcherSourceBase
    {
        /// <summary>
        /// Create a League of Legends replay file source with custom options
        /// </summary>
        /// <param name="options">Configuration options. If Path is not specified, uses default League of Legends replay location. If Patterns is not specified, uses "*.rofl".</param>
        public LeagueOfLegendsReplayFileSource(FileWatcherOptions options)
            : base(ApplyDefaults(options))
        {
        }

        /// <summary>
        /// Create a League of Legends replay file source
        /// </summary>
        /// <param name="customPath">Optional custom replay folder path. If null, uses default League of Legends replay location.</param>
        public LeagueOfLegendsReplayFileSource(string? customPath = null)
            : base(
                path: customPath ?? GetDefaultReplayPath(),
                pattern: "*.rofl",
                includeSubdirectories: false,
                debounceDelay: TimeSpan.FromSeconds(2))
        {
        }

        private static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return new FileWatcherOptions
            {
                Path = string.IsNullOrEmpty(options.Path) ? GetDefaultReplayPath() : options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? new[] { "*.rofl" }
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories,
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(2)
                    : options.DebounceDelay
            };
        }

        /// <summary>
        /// Get the default League of Legends replay folder path
        /// </summary>
        public static string GetDefaultReplayPath()
        {
            // Primary location: C:\Riot Games\League of Legends\Game\Replays
            var primaryPath = System.IO.Path.Combine("C:\\", "Riot Games", "League of Legends", "Game", "Replays");

            if (Directory.Exists(primaryPath))
            {
                return primaryPath;
            }

            // Fallback: Try Program Files
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles, Environment.SpecialFolderOption.DoNotVerify);
            var fallbackPath = System.IO.Path.Combine(programFiles, "Riot Games", "League of Legends", "Game", "Replays");

            if (Directory.Exists(fallbackPath))
            {
                return fallbackPath;
            }

            // Fallback: Try Program Files (x86)
            var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.DoNotVerify);
            var fallbackPathX86 = System.IO.Path.Combine(programFilesX86, "Riot Games", "League of Legends", "Game", "Replays");

            if (Directory.Exists(fallbackPathX86))
            {
                return fallbackPathX86;
            }

            // Return primary path as default even if it doesn't exist
            return primaryPath;
        }

        /// <summary>
        /// Validate that the League of Legends replay path exists and provide helpful error message
        /// </summary>
        protected override void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(
                    $"League of Legends replay folder not found: {path}\n" +
                    "Make sure League of Legends is installed and you've played at least one game.\n" +
                    "Expected location: C:\\Riot Games\\League of Legends\\Game\\Replays");
        }
    }
}
