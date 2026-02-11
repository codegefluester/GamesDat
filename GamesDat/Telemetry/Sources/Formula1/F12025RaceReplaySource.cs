namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    /// <summary>
    /// File watcher source for F1 2025 race replay files.
    /// Monitors: Documents\My Games\F1 25\replays
    /// </summary>
    public class F12025RaceReplaySource : F1RaceReplaySourceBase
    {
        public F12025RaceReplaySource(FileWatcherOptions options)
            : base(ApplyDefaults(EnsurePath(options)))
        {
        }

        public F12025RaceReplaySource(string? customPath = null)
            : base(CreateDefaultOptions(customPath ?? GetDefaultReplayPath()))
        {
        }

        public static string GetDefaultReplayPath() => GetReplayPathForYear("25");

        /// <summary>
        /// Ensure options has a path set, defaulting to F1 25 if not provided
        /// </summary>
        private static FileWatcherOptions EnsurePath(FileWatcherOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                options.Path = GetDefaultReplayPath();
            }
            return options;
        }

        /// <summary>
        /// Apply F1 25-specific defaults. Used by test discovery.
        /// </summary>
        private static new FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return F1RaceReplaySourceBase.ApplyDefaults(EnsurePath(options));
        }
    }
}
