using OneOf;
using ValveKeyValue;

namespace GamesDat.Core.Steam;

/// <summary>
/// Parses Steam's libraryfolders.vdf file to access installed games and library metadata.
/// All data is immutable after parsing.
/// </summary>
public sealed class SteamLibraryParser
{
    private readonly IReadOnlyDictionary<uint, SteamGame> _games;
    private readonly IReadOnlyList<SteamLibraryFolder> _libraryFolders;

    private SteamLibraryParser(
        IReadOnlyDictionary<uint, SteamGame> games,
        IReadOnlyList<SteamLibraryFolder> libraryFolders)
    {
        _games = games;
        _libraryFolders = libraryFolders;
    }

    /// <summary>
    /// Synchronously parses the libraryfolders.vdf file.
    /// </summary>
    /// <param name="vdfFilePath">Full path to the libraryfolders.vdf file.</param>
    /// <returns>A result containing either the parsed library or an error.</returns>
    public static SteamLibraryParseResult Parse(string vdfFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vdfFilePath);

        if (!File.Exists(vdfFilePath))
        {
            return new SteamLibraryParseResult(new FileNotFoundError(vdfFilePath));
        }

        try
        {
            using var stream = File.OpenRead(vdfFilePath);
            var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            var data = deserializer.Deserialize(stream);

            var parseResult = ParseLibraryData(data);
            return parseResult.Match(
                parser => new SteamLibraryParseResult(parser),
                _ => new SteamLibraryParseResult(new FileNotFoundError(vdfFilePath)),
                error => new SteamLibraryParseResult(error));
        }
        catch (Exception ex)
        {
            return new SteamLibraryParseResult(new ParseError($"Failed to parse VDF file: {ex.Message}"));
        }
    }

    /// <summary>
    /// Asynchronously parses the libraryfolders.vdf file.
    /// </summary>
    /// <param name="vdfFilePath">Full path to the libraryfolders.vdf file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task containing a result with either the parsed library or an error.</returns>
    public static async Task<SteamLibraryParseResult> ParseAsync(
        string vdfFilePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vdfFilePath);

        if (!File.Exists(vdfFilePath))
        {
            return new SteamLibraryParseResult(new FileNotFoundError(vdfFilePath));
        }

        try
        {
            return await Task.Run(() =>
            {
                using var stream = File.OpenRead(vdfFilePath);
                var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
                var data = deserializer.Deserialize(stream);
                var parseResult = ParseLibraryData(data);
                return parseResult.Match(
                    parser => new SteamLibraryParseResult(parser),
                    _ => new SteamLibraryParseResult(new FileNotFoundError(vdfFilePath)),
                    error => new SteamLibraryParseResult(error));
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            return new SteamLibraryParseResult(new ParseError($"Failed to parse VDF file: {ex.Message}"));
        }
    }

    /// <summary>
    /// Attempts to get a game by its Steam App ID.
    /// </summary>
    /// <param name="steamId">The Steam App ID of the game.</param>
    /// <returns>A result containing either the game or an error.</returns>
    public SteamGameLookupResult TryGetGame(uint steamId)
    {
        return _games.TryGetValue(steamId, out var game)
            ? new SteamGameLookupResult(game)
            : new SteamGameLookupResult(new GameNotFoundError(steamId));
    }

    /// <summary>
    /// Gets all installed games across all library folders.
    /// </summary>
    /// <returns>A read-only collection of all games.</returns>
    public IReadOnlyCollection<SteamGame> GetAllGames() => (IReadOnlyCollection<SteamGame>)_games.Values;

    /// <summary>
    /// Gets all library folders with their metadata.
    /// </summary>
    /// <returns>A read-only list of library folders.</returns>
    public IReadOnlyList<SteamLibraryFolder> GetLibraryFolders() => _libraryFolders;

    private static OneOf<SteamLibraryParser, FileNotFoundError, ParseError> ParseLibraryData(KVObject data)
    {
        try
        {
            var libraryFolders = new List<SteamLibraryFolder>();
            var allGames = new Dictionary<uint, SteamGame>();

            foreach (var libraryEntry in data.Children)
            {
                // Skip non-numeric keys (like "contentstatsid")
                if (!int.TryParse(libraryEntry.Name, out _))
                {
                    continue;
                }

                // Get the library folder properties
                string? path = null;
                string label = string.Empty;
                string? contentStatsId = null;
                ulong? totalSize = null;
                var apps = new List<SteamGame>();

                foreach (var prop in libraryEntry.Children)
                {
                    switch (prop.Name.ToLowerInvariant())
                    {
                        case "path":
                            path = prop.Value.ToString();
                            break;
                        case "label":
                            label = prop.Value.ToString() ?? string.Empty;
                            break;
                        case "contentstatsid":
                            contentStatsId = prop.Value.ToString();
                            break;
                        case "totalsize":
                            totalSize = TryParseULong(prop.Value.ToString());
                            break;
                        case "apps":
                            // Parse the apps collection
                            foreach (var app in prop.Children)
                            {
                                if (uint.TryParse(app.Name, out var appId))
                                {
                                    var sizeOnDisk = TryParseULong(app.Value.ToString());
                                    var game = new SteamGame(appId, sizeOnDisk, path ?? string.Empty);
                                    apps.Add(game);
                                    allGames[appId] = game;
                                }
                            }
                            break;
                    }
                }

                if (string.IsNullOrEmpty(path))
                {
                    throw new InvalidOperationException($"Library folder {libraryEntry.Name} missing path");
                }

                var libraryFolder = new SteamLibraryFolder(
                    path,
                    label,
                    contentStatsId,
                    totalSize,
                    apps);

                libraryFolders.Add(libraryFolder);
            }

            return new SteamLibraryParser(allGames, libraryFolders);
        }
        catch (Exception ex)
        {
            return new ParseError($"Failed to parse library structure: {ex.Message}");
        }
    }

    private static ulong? TryParseULong(string? value)
    {
        return ulong.TryParse(value, out var result) ? result : null;
    }
}

/// <summary>
/// Represents an installed Steam game.
/// </summary>
/// <param name="AppId">The Steam App ID.</param>
/// <param name="SizeOnDisk">The size of the game on disk in bytes, if available.</param>
/// <param name="LibraryPath">The path to the library folder containing this game.</param>
public sealed record SteamGame(
    uint AppId,
    ulong? SizeOnDisk,
    string LibraryPath)
{
    /// <summary>
    /// Gets the full path to the game's installation directory.
    /// </summary>
    public string InstallPath => Path.Combine(LibraryPath, "steamapps", "common");
}

/// <summary>
/// Represents a Steam library folder with its metadata and installed games.
/// </summary>
/// <param name="Path">The full path to the library folder.</param>
/// <param name="Label">The user-defined label for this library, if any.</param>
/// <param name="ContentStatsId">The content statistics ID, if available.</param>
/// <param name="TotalSize">The total size of the library in bytes, if available.</param>
/// <param name="Games">The collection of games installed in this library folder.</param>
public sealed record SteamLibraryFolder(
    string Path,
    string Label,
    string? ContentStatsId,
    ulong? TotalSize,
    IReadOnlyList<SteamGame> Games);

/// <summary>
/// Error indicating the VDF file was not found.
/// </summary>
/// <param name="FilePath">The path that was not found.</param>
public sealed record FileNotFoundError(string FilePath)
{
    public string Message => $"File not found: {FilePath}";
}

/// <summary>
/// Error indicating the VDF file could not be parsed.
/// </summary>
/// <param name="Message">Description of the parse error.</param>
public sealed record ParseError(string Message);

/// <summary>
/// Error indicating a game with the specified Steam ID was not found.
/// </summary>
/// <param name="SteamId">The Steam App ID that was not found.</param>
public sealed record GameNotFoundError(uint SteamId)
{
    public string Message => $"Game with Steam ID {SteamId} not found in library";
}
