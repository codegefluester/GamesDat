using System;
using System.IO;

namespace GamesDat.Core.Telemetry.Sources.Fortnite
{
    /// <summary>
    /// Watches for new Fortnite replay files in the default or custom location.
    /// Default location: %LOCALAPPDATA%\FortniteGame\Saved\Demos
    /// </summary>
    public class FortniteReplayFileSource : FileWatcherSourceBase
    {
        public FortniteReplayFileSource(FileWatcherOptions options) : base(options)
        {
        }

        public FortniteReplayFileSource(string? customPath = null) : base(ApplyDefaults(customPath))
        {
        }

        private static FileWatcherOptions ApplyDefaults(string? customPath)
        {
            return new FileWatcherOptions
            {
                Path = customPath ?? GetDefaultReplayPath(),
                Patterns = new[] { "*.replay" },
                IncludeSubdirectories = false,
                DebounceDelay = TimeSpan.FromSeconds(2)
            };
        }

        public static string GetDefaultReplayPath()
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
            return System.IO.Path.Combine(localAppData, "FortniteGame", "Saved", "Demos");
        }

        protected override void ValidatePath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException(
                    $"Fortnite replay directory not found at: {path}. " +
                    "Make sure Fortnite is installed and has been launched at least once.");
            }
        }
    }
}
