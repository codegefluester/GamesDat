using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesDat.Core.Telemetry.Sources.AgeOfEmpires4
{
    public class AgeOfEmpires4ReplayFileSource : FileWatcherSourceBase
    {
        public AgeOfEmpires4ReplayFileSource(FileWatcherOptions options) : base(ApplyDefaults(options))
        {
        }

        public AgeOfEmpires4ReplayFileSource()
            : base(
                  path: GetDefaultReplayPath(),
                  pattern: "*.*",
                  includeSubdirectories: false,
                  debounceDelay: TimeSpan.FromSeconds(2)
            )
        {
        }

        public static string GetDefaultReplayPath()
        {
            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);
            return System.IO.Path.Combine(documentsFolder, "My Games", "Age of Empires IV", "playback");
        }

        /// <summary>
        /// Applies default configuration options if not specified.
        /// </summary>
        /// <param name="options">The input options.</param>
        /// <returns>Options with defaults applied.</returns>
        private static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return new FileWatcherOptions
            {
                Path = string.IsNullOrEmpty(options.Path) ? GetDefaultReplayPath() : options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? ["*.*"]
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories,
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(2)
                    : options.DebounceDelay
            };
        }
    }
}
