using System;
using System.IO;
using GamesDat.Core.Helpers;
using GamesDat.Core.Steam;

namespace GamesDat.Core.Telemetry.Sources.DOTA2
{
    /// <summary>
    /// Watches for new DOTA 2 replay files in the default or custom location.
    /// Default location: Uses Steam library locator to find game installation, then appends "dota 2 beta\game\dota\replays"
    /// Fallback location: %PROGRAMFILES(X86)%\Steam\steamapps\common\dota 2 beta\game\dota\replays
    /// </summary>
    public class DOTA2ReplayFileSource : FileWatcherSourceBase
    {
        public const int DOTA2SteamAppId = 570;

        public DOTA2ReplayFileSource(FileWatcherOptions options) : base(options)
        {
        }

        public DOTA2ReplayFileSource(string? customPath = null) : base(ApplyDefaults(customPath))
        {
        }

        private static FileWatcherOptions ApplyDefaults(string? customPath)
        {
            return new FileWatcherOptions
            {
                Path = customPath ?? GetDefaultReplayPath(),
                Patterns = new[] { "*.dem" },
                IncludeSubdirectories = false,
                DebounceDelay = TimeSpan.FromSeconds(2)
            };
        }

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
            var game = parser.TryGetGame(DOTA2SteamAppId);
            if (game.IsError)
            {
                // Fallback to common installation path
                var steamPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.DoNotVerify);
                return System.IO.Path.Combine(steamPath, "Steam", "steamapps", "common", "dota 2 beta", "game", "dota", "replays");
            }

            return System.IO.Path.Combine(game.Game.InstallPath, "dota 2 beta", "game", "dota", "replays");
        }

        protected override void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(
                    $"DOTA 2 replay directory not found at: {path}. " +
                    "Make sure DOTA 2 is installed via Steam and has been launched at least once.");
            }
        }
    }
}
