namespace GamesDat.Core.Telemetry.Sources.iRacing
{
    /// <summary>
    /// File watcher source specifically configured for iRacing replay files
    /// </summary>
    public class iRacingreplayFileSource : FileWatcherSourceBase
    {

        /// <summary>
        /// Create a iRacing IBT file source with custom options
        /// </summary>
        /// <param name="options">Configuration options. If Path is not specified, uses default iRacing replay location. If Patterns is not specified, uses "*.rpy".</param>
        public iRacingreplayFileSource(FileWatcherOptions options)
            : base(ApplyDefaults(options))
        {
        }

        /// <summary>
        /// Create a iRacing replay file source
        /// </summary>
        /// <param name="customPath">Optional custom replay folder path. If null, uses default iRacing replay location.</param>
        public iRacingreplayFileSource(string? customPath = null)
            : base(
                path: customPath ?? GetDefaultTelemetryPath(),
                pattern: "*.rpy", // iRacing telemetry files
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
                    ? new[] { "*.rpy" }
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories,
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(3)
                    : options.DebounceDelay
            };
        }

        /// <summary>
        /// Get the default iRacing replay folder path
        /// </summary>
        public static string GetDefaultTelemetryPath()
        {
            var docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify);
            return System.IO.Path.Combine(docsPath, "iRacing", "replay");
        }
    }
}
