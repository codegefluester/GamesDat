namespace GamesDat.Core.Telemetry.Sources.Brawlhalla;

/// <summary>
/// File watcher source for Brawlhalla replay files.
/// Monitors the BrawlhallaReplays folder for new .replay files.
/// Brawlhalla automatically saves the last 25 matches without user action.
/// </summary>
public class BrawlhallaReplayFileSource : FileWatcherSourceBase
{
    /// <summary>
    /// Creates a new Brawlhalla replay file source with the specified options.
    /// </summary>
    /// <param name="options">Configuration options for the file watcher.</param>
    public BrawlhallaReplayFileSource(FileWatcherOptions options) : base(ApplyDefaults(options))
    {
    }

    /// <summary>
    /// Creates a new Brawlhalla replay file source.
    /// </summary>
    /// <param name="customPath">Optional custom path to monitor. If null, uses the default Brawlhalla replay folder.</param>
    /// <param name="includeSubdirectories">Whether to monitor subdirectories. Default is false.</param>
    public BrawlhallaReplayFileSource(string? customPath = null, bool includeSubdirectories = false)
        : base(
            path: customPath ?? GetDefaultReplayPath(),
            pattern: "*.replay",
            includeSubdirectories: includeSubdirectories,
            debounceDelay: TimeSpan.FromSeconds(2))
    {
    }

    /// <summary>
    /// Gets the default path where Brawlhalla saves replay files.
    /// </summary>
    /// <returns>The path to the BrawlhallaReplays folder.</returns>
    public static string GetDefaultReplayPath()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return System.IO.Path.Combine(userProfile, "BrawlhallaReplays");
    }

    /// <summary>
    /// Validates that the Brawlhalla replay folder exists.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown when the replay folder doesn't exist.</exception>
    protected override void ValidatePath(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException(
                $"Brawlhalla replay folder not found: {path}\n" +
                "Make sure Brawlhalla is installed and you've played at least one match.\n" +
                "Replays are automatically saved to C:\\Users\\[Username]\\BrawlhallaReplays");
        }
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
                ? ["*.replay"]
                : options.Patterns,
            IncludeSubdirectories = options.IncludeSubdirectories,
            DebounceDelay = options.DebounceDelay == default
                ? TimeSpan.FromSeconds(2)
                : options.DebounceDelay
        };
    }
}
