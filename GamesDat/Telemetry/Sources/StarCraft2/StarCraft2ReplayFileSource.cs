using System;
using System.IO;

namespace GamesDat.Core.Telemetry.Sources.StarCraft2
{
    /// <summary>
    /// File watcher source specifically configured for StarCraft 2 replay files
    /// </summary>
    public class StarCraft2ReplayFileSource : FileWatcherSourceBase
    {
        /// <summary>
        /// Create a StarCraft 2 replay file source with custom options
        /// </summary>
        /// <param name="options">Configuration options. If Path is not specified, uses default StarCraft 2 replay location. If Patterns is not specified, uses "*.SC2Replay".</param>
        public StarCraft2ReplayFileSource(FileWatcherOptions options)
            : base(ApplyDefaults(options))
        {
        }

        /// <summary>
        /// Create a StarCraft 2 replay file source
        /// </summary>
        /// <param name="customPath">Optional custom replay folder path. If null, uses default StarCraft 2 replay location.</param>
        public StarCraft2ReplayFileSource(string? customPath = null)
            : base(
                path: customPath ?? GetDefaultReplayPath(),
                pattern: "*.SC2Replay",
                includeSubdirectories: true, // SC2 stores replays in subdirectories by account
                debounceDelay: TimeSpan.FromSeconds(2))
        {
        }

        private static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return new FileWatcherOptions
            {
                Path = string.IsNullOrEmpty(options.Path) ? GetDefaultReplayPath() : options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? new[] { "*.SC2Replay" }
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories || true, // Default to true for SC2
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(2)
                    : options.DebounceDelay
            };
        }

        /// <summary>
        /// Get the default StarCraft 2 replay folder path
        /// StarCraft 2 stores replays in: Documents\StarCraft II\Accounts\[ID]\[#-S2-#-######]\Replays
        /// We return the base Accounts folder and rely on IncludeSubdirectories to find replays
        /// </summary>
        public static string GetDefaultReplayPath()
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);
            var sc2Path = System.IO.Path.Combine(documents, "StarCraft II", "Accounts");

            // If the Accounts folder doesn't exist, try the base StarCraft II folder
            if (!Directory.Exists(sc2Path))
            {
                sc2Path = System.IO.Path.Combine(documents, "StarCraft II");
            }

            return sc2Path;
        }

        /// <summary>
        /// Validate that the StarCraft 2 replay path exists and provide helpful error message
        /// </summary>
        protected override void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(
                    $"StarCraft 2 replay folder not found: {path}\n" +
                    "Make sure StarCraft 2 is installed and you've played at least one game.\n" +
                    "Expected location: Documents\\StarCraft II\\Accounts\\[ID]\\[#-S2-#-######]\\Replays");
        }
    }
}
