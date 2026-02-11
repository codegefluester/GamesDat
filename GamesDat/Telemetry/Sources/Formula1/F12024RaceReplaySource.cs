namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    /// <summary>
    /// File watcher source for F1 2024 race replay files.
    /// Monitors: Documents\My Games\F1 24\replays
    /// </summary>
    public class F12024RaceReplaySource : F1RaceReplaySourceBase
    {
        public F12024RaceReplaySource(FileWatcherOptions options)
            : base(ApplyDefaults(options))
        {
        }

        public F12024RaceReplaySource(string? customPath = null)
            : base(CreateDefaultOptions(customPath ?? GetDefaultReplayPath()))
        {
        }

        public static string GetDefaultReplayPath() => GetReplayPathForYear("24");

        /// <summary>
        /// Ensure options has a path set, defaulting to F1 24 if not provided
        /// </summary>
        private static FileWatcherOptions EnsurePath(FileWatcherOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                return new FileWatcherOptions
                {
                    Path = GetDefaultReplayPath(),
                    Patterns = options.Patterns,
                    IncludeSubdirectories = options.IncludeSubdirectories,
                    DebounceDelay = options.DebounceDelay
                };
            }
            return options;
        }

        /// <summary>
        /// Apply F1 24-specific defaults. Used by test discovery.
        /// </summary>
        private static new FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return F1RaceReplaySourceBase.ApplyDefaults(EnsurePath(options));
        }
    }
}
