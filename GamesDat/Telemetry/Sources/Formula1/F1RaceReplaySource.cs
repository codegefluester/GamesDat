using System;
using System.IO;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    /// <summary>
    /// File watcher source for F1 race replay files.
    /// Defaults to F1 25, but can auto-detect the latest installed F1 game.
    /// For monitoring specific years or multiple years simultaneously, use year-specific sources:
    /// F12025RaceReplaySource, F12024RaceReplaySource, F12023RaceReplaySource, F12022RaceReplaySource
    /// </summary>
    public class F1RaceReplaySource : F1RaceReplaySourceBase
    {
        public F1RaceReplaySource(FileWatcherOptions options)
            : base(ApplyDefaults(EnsurePath(options)))
        {
        }

        /// <summary>
        /// Ensure options has a path set, defaulting to F1 25 if not provided
        /// </summary>
        private static FileWatcherOptions EnsurePath(FileWatcherOptions options)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                options.Path = F12025RaceReplaySource.GetDefaultReplayPath();
            }
            return options;
        }

        /// <summary>
        /// Apply F1 defaults. Used by test discovery.
        /// </summary>
        private static new FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return F1RaceReplaySourceBase.ApplyDefaults(EnsurePath(options));
        }

        /// <summary>
        /// Create F1 replay source with optional custom path and detection strategy
        /// </summary>
        /// <param name="customPath">Custom replay folder path. If provided, autoDetectLatestInstalled is ignored.</param>
        /// <param name="autoDetectLatestInstalled">If true and customPath is null, auto-detects the latest installed F1 game (checks 25, 24, 23, 22). If false, defaults to F1 25.</param>
        public F1RaceReplaySource(string? customPath = null, bool autoDetectLatestInstalled = false)
            : base(CreateDefaultOptions(ResolveReplayPath(customPath, autoDetectLatestInstalled)))
        {
        }

        /// <summary>
        /// Get the default F1 replay folder path (F1 25)
        /// </summary>
        public static string GetDefaultReplayPath() => F12025RaceReplaySource.GetDefaultReplayPath();

        /// <summary>
        /// Auto-detect the latest installed F1 game's replay folder.
        /// Checks F1 25, F1 24, F1 23, F1 22 in order and returns the first that exists.
        /// Note: This checks for folder existence, which may include cases where the game
        /// is uninstalled but replay folders remain.
        /// </summary>
        public static string GetLatestInstalledReplayPath()
        {
            var years = new[] { "25", "24", "23", "22" };
            foreach (var year in years)
            {
                var path = GetReplayPathForYear(year);
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            // Default to F1 25 even if it doesn't exist
            return GetReplayPathForYear("25");
        }

        private static string ResolveReplayPath(string? customPath, bool autoDetectLatestInstalled)
        {
            if (customPath != null)
            {
                return customPath;
            }

            if (autoDetectLatestInstalled)
            {
                return GetLatestInstalledReplayPath();
            }

            return GetDefaultReplayPath();
        }
    }
}
