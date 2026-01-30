using System;
using System.IO;

namespace GamesDat.Core.Telemetry.Sources.Overwatch2
{
    /// <summary>
    /// Watches for new Overwatch 2 highlight video files in the default or custom location.
    /// Default location: %USERPROFILE%\Documents\Overwatch\videos\overwatch
    /// Note: Overwatch 2 exports highlights as MP4 videos rather than traditional replay files.
    /// </summary>
    public class Overwatch2HighlightFileSource : FileWatcherSourceBase
    {
        public const int Overwatch2SteamAppId = 2357570;

        public Overwatch2HighlightFileSource(FileWatcherOptions options) : base(options)
        {
        }

        public Overwatch2HighlightFileSource(string? customPath = null) : base(ApplyDefaults(customPath))
        {
        }

        private static FileWatcherOptions ApplyDefaults(string? customPath)
        {
            return new FileWatcherOptions
            {
                Path = customPath ?? GetDefaultReplayPath(),
                Patterns = new[] { "*.mp4" },
                IncludeSubdirectories = false,
                DebounceDelay = TimeSpan.FromSeconds(3) // Video files take time to write
            };
        }

        public static string GetDefaultReplayPath()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);
            return System.IO.Path.Combine(documentsPath, "Overwatch", "videos", "overwatch");
        }

        protected override void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(
                    $"Overwatch 2 highlights directory not found at: {path}. " +
                    "Make sure Overwatch 2 is installed and has been launched at least once. " +
                    "Highlights must be exported from the game to appear in this folder.");
            }
        }
    }
}
