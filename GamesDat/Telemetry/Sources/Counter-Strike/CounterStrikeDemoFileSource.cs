using GamesDat.Core.Helpers;
using GamesDat.Core.Steam;
using System.Diagnostics;

namespace GamesDat.Core.Telemetry.Sources.Counter_Strike
{
    public class CounterStrikeDemoFileSource : FileWatcherSourceBase
    {
        public const uint CounterStrikeSteamAppId = 730;

        public CounterStrikeDemoFileSource(FileWatcherOptions options) : base(ApplyDefaults(options))
        {
        }

        public CounterStrikeDemoFileSource(string? customPath = null)
            : base(path: customPath ?? GetDefaultDemoPath(),
                pattern: "*.dem",
                includeSubdirectories: false,
                debounceDelay: TimeSpan.FromSeconds(2))
        { }

        private static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return new FileWatcherOptions
            {
                Path = string.IsNullOrEmpty(options.Path) ? GetDefaultDemoPath() : options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? ["*.dem"]
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories,
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(1)
                    : options.DebounceDelay
            };
        }

        public static string GetDefaultDemoPath()
        {
            // Step 1: Get steam library path(s)
            var steamLibraryPath = SteamPathLocator.GetSteamVDFPath();
            if (steamLibraryPath.IsError)
            {
                throw new InvalidOperationException($"Could not locate Steam library folders: {steamLibraryPath.Error.Message}");
            }

            // Step 2: Parse all libraries
            Debug.WriteLine("Located Steam libraries:");
            var parserResult = SteamLibraryParser.Parse(steamLibraryPath.Path);
            if (parserResult.IsError)
            {
                throw new InvalidOperationException($"Could not parse Steam library folders: {parserResult.Error}");
            }

            // Step 3: Look for CS:GO using its Steam AppID (730)
            var parser = parserResult.Parser;
            var game = parser.TryGetGame(CounterStrikeSteamAppId);
            if (game.IsError)
            {
                var steamPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.DoNotVerify);
                return System.IO.Path.Combine(steamPath, "Steam", "steamapps", "common", "Counter-Strike Global Offensive", "csgo");
            }

            Debug.WriteLine($"Found CS:GO in Steam libraries: {game.Game.InstallPath}");

            // Step 4: If found, construct the demo path
            return System.IO.Path.Combine(game.Game.InstallPath, "Counter-Strike Global Offensive", "game", "csgo");
        }
    }
}
