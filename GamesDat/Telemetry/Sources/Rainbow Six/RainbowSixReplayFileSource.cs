using System;
using System.IO;

namespace GamesDat.Core.Telemetry.Sources.Rainbow_Six
{
    /// <summary>
    /// File watcher source specifically configured for Rainbow Six Siege replay files
    /// </summary>
    public class RainbowSixReplayFileSource : FileWatcherSourceBase
    {
        /// <summary>
        /// Create a Rainbow Six Siege replay file source with custom options
        /// </summary>
        /// <param name="options">Configuration options. If Path is not specified, uses default Rainbow Six Siege replay location. If Patterns is not specified, uses "*.rec".</param>
        public RainbowSixReplayFileSource(FileWatcherOptions options)
            : base(ApplyDefaults(options))
        {
        }

        /// <summary>
        /// Create a Rainbow Six Siege replay file source
        /// </summary>
        /// <param name="customPath">Optional custom replay folder path. If null, uses default Rainbow Six Siege replay location.</param>
        public RainbowSixReplayFileSource(string? customPath = null)
            : base(
                path: customPath ?? GetDefaultReplayPath(),
                pattern: "*.rec", // R6 Siege replay files
                includeSubdirectories: false,
                debounceDelay: TimeSpan.FromSeconds(3))
        {
        }

        private static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return new FileWatcherOptions
            {
                Path = string.IsNullOrEmpty(options.Path) ? GetDefaultReplayPath() : options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? new[] { "*.rec" }
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories,
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(3)
                    : options.DebounceDelay
            };
        }

        /// <summary>
        /// Get the default Rainbow Six Siege replay folder path
        /// </summary>
        public static string GetDefaultReplayPath()
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            // R6 stores replays in Documents\My Games\Rainbow Six - Siege\[profile_id]\replays
            // We'll need to scan for profile folders
            var baseGamePath = System.IO.Path.Combine(userProfile, "Documents", "My Games", "Rainbow Six - Siege");

            if (!Directory.Exists(baseGamePath))
                throw new DirectoryNotFoundException($"Rainbow Six Siege game folder not found: {baseGamePath}");

            // Find first profile folder with replays subfolder
            var profileDirs = Directory.GetDirectories(baseGamePath);
            foreach (var profileDir in profileDirs)
            {
                var replaysPath = System.IO.Path.Combine(profileDir, "replays");
                if (Directory.Exists(replaysPath))
                    return replaysPath;
            }

            throw new DirectoryNotFoundException("Rainbow Six Siege replay folder not found in any profile");
        }
    }
}
