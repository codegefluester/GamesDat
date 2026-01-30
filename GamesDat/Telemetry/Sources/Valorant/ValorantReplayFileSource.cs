using System;
using System.IO;

namespace GamesDat.Core.Telemetry.Sources.Valorant
{
    /// <summary>
    /// Watches for new Valorant replay files in the default or custom location.
    /// Default location: %LOCALAPPDATA%\VALORANT\Saved\Demos
    /// Note: Valorant replays expire after 21 days or when a new patch is released.
    /// </summary>
    public class ValorantReplayFileSource : FileWatcherSourceBase
    {
        public ValorantReplayFileSource(FileWatcherOptions options) : base(options)
        {
        }

        public ValorantReplayFileSource(string? customPath = null) : base(ApplyDefaults(customPath))
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
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
            return System.IO.Path.Combine(localAppData, "VALORANT", "Saved", "Demos");
        }

        protected override void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(
                    $"Valorant replay directory not found at: {path}. " +
                    "Make sure Valorant is installed and has been launched at least once. " +
                    "Note: Replays expire after 21 days or when a new patch is released.");
            }
        }
    }
}
