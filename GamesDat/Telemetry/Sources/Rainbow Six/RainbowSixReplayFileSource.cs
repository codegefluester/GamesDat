using GamesDat.Core.Helpers;
using GamesDat.Core.Steam;

namespace GamesDat.Core.Telemetry.Sources.Rainbow_Six
{
    /// <summary>
    /// File watcher source specifically configured for Rainbow Six Siege replay files
    /// </summary>
    public class RainbowSixReplayFileSource : FileWatcherSourceBase
    {
        public const uint RainbowSixSiegeSteamAppId = 359550;

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
                includeSubdirectories: true,
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
            var steamLibraryPath = SteamPathLocator.GetSteamVDFPath();
            if (steamLibraryPath.IsError)
            {
                throw new InvalidOperationException($"Could not locate Steam library folders: {steamLibraryPath.Error.Message}");
            }

            var parserResult = SteamLibraryParser.Parse(steamLibraryPath.Path);
            if (parserResult.IsError)
            {
                throw new InvalidOperationException($"Could not parse Steam library folders: {parserResult.Error}");
            }

            var parser = parserResult.Parser;
            var game = parser.TryGetGame(RainbowSixSiegeSteamAppId);
            if (game.IsError)
            {
                var steamPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.DoNotVerify);
                return System.IO.Path.Combine(steamPath, "Steam", "steamapps", "common", "Tom Clancy's Rainbow Six Siege", "MatchReplay");
            }

            return System.IO.Path.Combine(game.Game.InstallPath, "Tom Clancy's Rainbow Six Siege", "MatchReplay");
        }
    }
}
