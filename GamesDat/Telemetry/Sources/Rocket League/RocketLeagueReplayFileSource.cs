using System;
using System.IO;

namespace GamesDat.Core.Telemetry.Sources.Rocket_League
{
    /// <summary>
    /// File watcher source specifically configured for Rocket League replay files
    /// </summary>
    public class RocketLeagueReplayFileSource : FileWatcherSourceBase
    {
        /// <summary>
        /// Create a Rocket League replay file source with custom options
        /// </summary>
        /// <param name="options">Configuration options. If Path is not specified, uses default Rocket League replay location. If Patterns is not specified, uses "*.replay".</param>
        public RocketLeagueReplayFileSource(FileWatcherOptions options)
            : base(ApplyDefaults(options))
        {
        }

        /// <summary>
        /// Create a Rocket League replay file source
        /// </summary>
        /// <param name="customPath">Optional custom replay folder path. If null, uses default Rocket League replay location.</param>
        public RocketLeagueReplayFileSource(string? customPath = null)
            : base(
                path: customPath ?? GetDefaultReplayPath(),
                pattern: "*.replay",
                includeSubdirectories: false,
                debounceDelay: TimeSpan.FromSeconds(2)) // RL writes replays in chunks
        {
        }

        private static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return new FileWatcherOptions
            {
                Path = string.IsNullOrEmpty(options.Path) ? GetDefaultReplayPath() : options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? new[] { "*.replay" }
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories,
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(2)
                    : options.DebounceDelay
            };
        }

        /// <summary>
        /// Get the default Rocket League replay folder path
        /// </summary>
        public static string GetDefaultReplayPath()
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);
            return System.IO.Path.Combine(userProfile, "Documents", "My Games", "Rocket League", "TAGame", "Demos");
        }

        /// <summary>
        /// Validate that the Rocket League replay path exists and provide helpful error message
        /// </summary>
        protected override void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(
                    $"Rocket League replay folder not found: {path}\n" +
                    "Make sure Rocket League is installed and you've played at least one game.");
        }
    }
}
