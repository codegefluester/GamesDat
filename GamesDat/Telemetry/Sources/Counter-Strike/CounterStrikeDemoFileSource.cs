namespace GamesDat.Core.Telemetry.Sources.Counter_Strike
{
    public class CounterStrikeDemoFileSource : FileWatcherSourceBase
    {
        public CounterStrikeDemoFileSource(FileWatcherOptions options) : base(ApplyDefaults(options))
        {
        }

        public CounterStrikeDemoFileSource(string? customPath = null)
            : base(path: customPath ?? GetDefaultDemoPath(),
                pattern: "*.dem",
                includeSubdirectories: false,
                debounceDelay: TimeSpan.FromSeconds(2))
        { }

        private static FileWatcherOptions ApplyDefaults(FileWatcherOptions options)
        {
            return new FileWatcherOptions
            {
                Path = string.IsNullOrEmpty(options.Path) ? GetDefaultDemoPath() : options.Path,
                Patterns = options.Patterns == null || options.Patterns.Length == 0
                    ? ["*.dem"]
                    : options.Patterns,
                IncludeSubdirectories = options.IncludeSubdirectories,
                DebounceDelay = options.DebounceDelay == default
                    ? TimeSpan.FromSeconds(1)
                    : options.DebounceDelay
            };
        }

        public static string GetDefaultDemoPath()
        {
            var steamPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.DoNotVerify);
            return System.IO.Path.Combine(steamPath, "Steam", "steamapps", "common", "Counter-Strike Global Offensive", "game", "csgo");
        }
    }
}
