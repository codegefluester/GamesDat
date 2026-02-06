namespace GamesDat.Core.Telemetry.Sources.iRacing
{
    /// <summary>
    /// File watcher source specifically configured for iRacing OLAP/BLAP files
    /// </summary>
    public class iRacingBestLapFileSource : FileWatcherSourceBase
    {

        /// <summary>
        /// Create a iRacing OLAP/BLAP file source with custom options
        /// </summary>
        /// <param name="options">Configuration options. If Path is not specified, uses default iRacing lapfiles location. If Patterns is not specified, uses "*.olap" and "*.blap".</param>
        public iRacingBestLapFileSource(FileWatcherOptions options)
            : base(ApplyDefaults(options))
        {
        }

        /// <summary>
        /// Create a iRacing lapfiles file source
        /// </summary>
        /// <param name="customPath">Optional custom lapfiles folder path. If null, uses default iRacing lapfiles location.</param>
        public iRacingBestLapFileSource(string? customPath = null)
            : base(
                path: customPath ?? GetDefaultTelemetryPath(),
                patterns: new[] { "*.olap", "*.blap" },
                includeSubdirectories: true,
                debounceDelay: TimeSpan.FromSeconds(3))
        {
        }

        private static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return new FileWatcherOptions
            {
                Path = string.IsNullOrEmpty(options.Path) ? GetDefaultTelemetryPath() : options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? new[] { "*.olap", "*.blap" }
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories,
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(3)
                    : options.DebounceDelay
            };
        }

        /// <summary>
        /// Get the default iRacing lapfiles folder path
        /// </summary>
        public static string GetDefaultTelemetryPath()
        {
            var docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);
            return System.IO.Path.Combine(docsPath, "iRacing", "lapfiles");
        }
    }
}
