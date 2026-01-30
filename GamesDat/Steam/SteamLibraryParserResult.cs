using OneOf;

namespace GamesDat.Core.Steam;

/// <summary>
/// Represents the result of attempting to parse a Steam library VDF file.
/// Contains either a successful parser instance or an error.
/// </summary>
public readonly struct SteamLibraryParseResult : IEquatable<SteamLibraryParseResult>
{
    private readonly OneOf<SteamLibraryParser, FileNotFoundError, ParseError> _value;

    /// <summary>
    /// Creates a successful result with the parser.
    /// </summary>
    /// <param name="parser">The successfully created parser instance.</param>
    public SteamLibraryParseResult(SteamLibraryParser parser)
    {
        ArgumentNullException.ThrowIfNull(parser);
        _value = parser;
    }

    /// <summary>
    /// Creates an error result with a file not found error.
    /// </summary>
    /// <param name="error">The file not found error.</param>
    public SteamLibraryParseResult(FileNotFoundError error)
    {
        _value = error;
    }

    /// <summary>
    /// Creates an error result with a parse error.
    /// </summary>
    /// <param name="error">The parse error.</param>
    public SteamLibraryParseResult(ParseError error)
    {
        _value = error;
    }

    /// <summary>
    /// Indicates whether the parsing was successful.
    /// </summary>
    public bool IsSuccess => _value.IsT0;

    /// <summary>
    /// Indicates whether an error occurred during parsing.
    /// </summary>
    public bool IsError => _value.IsT1 || _value.IsT2;

    /// <summary>
    /// Gets the parser if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Parser on an error result.</exception>
    public SteamLibraryParser Parser => _value.IsT0
        ? _value.AsT0
        : throw new InvalidOperationException("Cannot access Parser on an error result. Check IsSuccess first.");

    /// <summary>
    /// Gets the error if parsing failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Error on a success result.</exception>
    public object Error => _value.IsT1
        ? _value.AsT1
        : _value.IsT2
            ? _value.AsT2
            : throw new InvalidOperationException("Cannot access Error on a success result. Check IsError first.");

    /// <summary>
    /// Executes one of two functions depending on whether the result is success or error.
    /// </summary>
    /// <typeparam name="TResult">The return type of the match functions.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful.</param>
    /// <param name="onError">Function to execute if the result is an error.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(
        Func<SteamLibraryParser, TResult> onSuccess,
        Func<object, TResult> onError)
    {
        return _value.Match(
            onSuccess,
            error => onError(error),
            error => onError(error));
    }

    /// <summary>
    /// Executes one of two actions depending on whether the result is success or error.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful.</param>
    /// <param name="onError">Action to execute if the result is an error.</param>
    public void Switch(
        Action<SteamLibraryParser> onSuccess,
        Action<object> onError)
    {
        _value.Switch(
            onSuccess,
            error => onError(error),
            error => onError(error));
    }

    /// <summary>
    /// Attempts to get the parser value.
    /// </summary>
    /// <param name="parser">The parser if successful; otherwise, null.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public bool TryGetParser(out SteamLibraryParser? parser)
    {
        if (_value.IsT0)
        {
            parser = _value.AsT0;
            return true;
        }

        parser = null;
        return false;
    }

    /// <summary>
    /// Attempts to get the error value.
    /// </summary>
    /// <param name="error">The error if failed; otherwise, null.</param>
    /// <returns>True if failed; otherwise, false.</returns>
    public bool TryGetError(out object? error)
    {
        if (_value.IsT1)
        {
            error = _value.AsT1;
            return true;
        }

        if (_value.IsT2)
        {
            error = _value.AsT2;
            return true;
        }

        error = null;
        return false;
    }

    public bool Equals(SteamLibraryParseResult other) => _value.Equals(other._value);
    public override bool Equals(object? obj) => obj is SteamLibraryParseResult other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(SteamLibraryParseResult left, SteamLibraryParseResult right) => left.Equals(right);
    public static bool operator !=(SteamLibraryParseResult left, SteamLibraryParseResult right) => !left.Equals(right);

    public override string ToString() => _value.Match(
        parser => $"Success: Parsed library with {parser.GetAllGames().Count} games",
        fnf => $"Error: {fnf.Message}",
        pe => $"Error: {pe.Message}");
}

/// <summary>
/// Represents the result of attempting to look up a game in the Steam library.
/// Contains either a successful game or an error.
/// </summary>
public readonly struct SteamGameLookupResult : IEquatable<SteamGameLookupResult>
{
    private readonly OneOf<SteamGame, GameNotFoundError> _value;

    /// <summary>
    /// Creates a successful result with the game.
    /// </summary>
    /// <param name="game">The found game.</param>
    public SteamGameLookupResult(SteamGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        _value = game;
    }

    /// <summary>
    /// Creates an error result.
    /// </summary>
    /// <param name="error">The error describing why the game could not be found.</param>
    public SteamGameLookupResult(GameNotFoundError error)
    {
        _value = error;
    }

    /// <summary>
    /// Indicates whether the game was successfully found.
    /// </summary>
    public bool IsSuccess => _value.IsT0;

    /// <summary>
    /// Indicates whether an error occurred during lookup.
    /// </summary>
    public bool IsError => _value.IsT1;

    /// <summary>
    /// Gets the game if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Game on an error result.</exception>
    public SteamGame Game => _value.IsT0
        ? _value.AsT0
        : throw new InvalidOperationException("Cannot access Game on an error result. Check IsSuccess first.");

    /// <summary>
    /// Gets the error if the lookup failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Error on a success result.</exception>
    public GameNotFoundError Error => _value.IsT1
        ? _value.AsT1
        : throw new InvalidOperationException("Cannot access Error on a success result. Check IsError first.");

    /// <summary>
    /// Executes one of two functions depending on whether the result is success or error.
    /// </summary>
    /// <typeparam name="TResult">The return type of the match functions.</typeparam>
    /// <param name="onSuccess">Function to execute if the result is successful.</param>
    /// <param name="onError">Function to execute if the result is an error.</param>
    /// <returns>The result of the executed function.</returns>
    public TResult Match<TResult>(
        Func<SteamGame, TResult> onSuccess,
        Func<GameNotFoundError, TResult> onError)
    {
        return _value.Match(onSuccess, onError);
    }

    /// <summary>
    /// Executes one of two actions depending on whether the result is success or error.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful.</param>
    /// <param name="onError">Action to execute if the result is an error.</param>
    public void Switch(
        Action<SteamGame> onSuccess,
        Action<GameNotFoundError> onError)
    {
        _value.Switch(onSuccess, onError);
    }

    /// <summary>
    /// Attempts to get the game value.
    /// </summary>
    /// <param name="game">The game if successful; otherwise, null.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public bool TryGetGame(out SteamGame? game)
    {
        if (_value.IsT0)
        {
            game = _value.AsT0;
            return true;
        }

        game = null;
        return false;
    }

    /// <summary>
    /// Attempts to get the error value.
    /// </summary>
    /// <param name="error">The error if failed; otherwise, default.</param>
    /// <returns>True if failed; otherwise, false.</returns>
    public bool TryGetError(out GameNotFoundError? error)
    {
        if (_value.IsT1)
        {
            error = _value.AsT1;
            return true;
        }

        error = null;
        return false;
    }

    public bool Equals(SteamGameLookupResult other) => _value.Equals(other._value);
    public override bool Equals(object? obj) => obj is SteamGameLookupResult other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(SteamGameLookupResult left, SteamGameLookupResult right) => left.Equals(right);
    public static bool operator !=(SteamGameLookupResult left, SteamGameLookupResult right) => !left.Equals(right);

    public override string ToString() => _value.Match(
        game => $"Success: {game.AppId}",
        error => $"Error: {error.Message}");
}
