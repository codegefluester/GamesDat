using System;
using System.IO;
using GamesDat.Core.Helpers;
using GamesDat.Core.Steam;

namespace GamesDat.Core.Telemetry.Sources.Tekken8
{
    /// <summary>
    /// Watches for new Tekken 8 replay files in the default or custom location.
    /// Default location: %LOCALAPPDATA%\TEKKEN 8\Saved\SaveGames\[SteamID]\
    /// Tekken 8 automatically saves the last 100 matches as individual files.
    ///
    /// Note: File extension is currently unknown and requires testing with actual game installation.
    /// Using wildcard pattern until confirmed (likely .sav, .trp, or .replay).
    ///
    /// STREET FIGHTER 6 NOTE:
    /// Street Fighter 6 was considered during this implementation but is NOT VIABLE for FileWatcherSource.
    /// Reasons:
    /// - Uses RE Engine binary database files (data00-1.bin, etc.) instead of individual replay files
    /// - Replays are aggregated in binary format, not discrete files
    /// - No file creation/modification events per replay
    /// - Cannot detect which replay is new from file modification
    /// - Competitive replays stored server-side only
    /// If SF6 support is required, it would need a custom binary parser source or memory-mapped approach.
    /// </summary>
    public class Tekken8ReplayFileSource : FileWatcherSourceBase
    {
        public const int Tekken8SteamAppId = 1778820;

        /// <summary>
        /// Create a Tekken 8 replay file source with custom options
        /// </summary>
        /// <param name="options">Configuration options. If Path is not specified, uses default Tekken 8 replay location.</param>
        public Tekken8ReplayFileSource(FileWatcherOptions options)
            : base(ApplyDefaults(options))
        {
        }

        /// <summary>
        /// Create a Tekken 8 replay file source
        /// </summary>
        /// <param name="customPath">Optional custom replay folder path. If null, uses default Tekken 8 SaveGames location.</param>
        public Tekken8ReplayFileSource(string? customPath = null)
            : base(ApplyDefaults(customPath))
        {
        }

        private static FileWatcherOptions ApplyDefaults(string? customPath)
        {
            return new FileWatcherOptions
            {
                Path = customPath ?? GetDefaultReplayPath(),
                // TODO: Update patterns once file extension is confirmed through testing
                // Likely candidates: *.sav, *.trp, *.replay
                Patterns = new[] { "*.*" },
                IncludeSubdirectories = true, // Replays stored in SteamID subdirectories
                DebounceDelay = TimeSpan.FromSeconds(2) // ~2.8MB files based on Tekken 7
            };
        }

        private static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return new FileWatcherOptions
            {
                Path = string.IsNullOrEmpty(options.Path) ? GetDefaultReplayPath() : options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? new[] { "*.*" }
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories,
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(2)
                    : options.DebounceDelay
            };
        }

        /// <summary>
        /// Get the default Tekken 8 SaveGames folder path.
        /// Returns parent directory to capture all SteamID subdirectories.
        /// </summary>
        public static string GetDefaultReplayPath()
        {
            // Try to locate via Steam first
            var steamLibraryPath = SteamPathLocator.GetSteamVDFPath();
            if (!steamLibraryPath.IsError)
            {
                var parserResult = SteamLibraryParser.Parse(steamLibraryPath.Path);
                if (!parserResult.IsError)
                {
                    var parser = parserResult.Parser;
                    var game = parser.TryGetGame(Tekken8SteamAppId);
                    if (!game.IsError)
                    {
                        // Game found via Steam, but replays are in LOCALAPPDATA not install dir
                        // Still validate it exists to provide better error message
                        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        return System.IO.Path.Combine(localAppData, "TEKKEN 8", "Saved", "SaveGames");
                    }
                }
            }

            // Fallback to standard location
            var fallbackLocalAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return System.IO.Path.Combine(fallbackLocalAppData, "TEKKEN 8", "Saved", "SaveGames");
        }

        protected override void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(
                    $"Tekken 8 SaveGames directory not found at: {path}. " +
                    "Make sure Tekken 8 is installed via Steam and has been launched at least once. " +
                    "The game automatically saves the last 100 matches. " +
                    "If you have multiple Steam library folders, check that Tekken 8 is installed in the expected location.");
            }
        }
    }
}
