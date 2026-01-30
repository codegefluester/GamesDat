using System;
using System.IO;

namespace GamesDat.Core.Telemetry.Sources.PUBG
{
    /// <summary>
    /// Watches for new PUBG: Battlegrounds replay files in the default or custom location.
    /// Default location: %LOCALAPPDATA%\TslGame\Saved\Demos
    /// Note: PUBG stores replays in dated subfolders within the Demos directory.
    /// </summary>
    public class PUBGReplayFileSource : FileWatcherSourceBase
    {
        public const int PUBGSteamAppId = 578080;

        public PUBGReplayFileSource(FileWatcherOptions options) : base(options)
        {
        }

        public PUBGReplayFileSource(string? customPath = null) : base(ApplyDefaults(customPath))
        {
        }

        private static FileWatcherOptions ApplyDefaults(string? customPath)
        {
            return new FileWatcherOptions
            {
                Path = customPath ?? GetDefaultReplayPath(),
                Patterns = new[] { "*.replayinfo" },
                IncludeSubdirectories = true, // Replays stored in dated folders
                DebounceDelay = TimeSpan.FromSeconds(3) // Large replay files written in chunks
            };
        }

        public static string GetDefaultReplayPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
            return System.IO.Path.Combine(localAppData, "TslGame", "Saved", "Demos");
        }

        protected override void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(
                    $"PUBG replay directory not found at: {path}. " +
                    "Make sure PUBG: Battlegrounds is installed and has been launched at least once.");
            }
        }
    }
}
