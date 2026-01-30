using System.Runtime.Versioning;
using Microsoft.Win32;
using OneOf;

namespace GamesDat.Core.Helpers;

/// <summary>
/// Provides methods to locate the Steam library folders VDF file on Windows systems.
/// </summary>
[SupportedOSPlatform("windows")]
public static class SteamPathLocator
{
    private static SteamVDFLocation? _cachedResult;
    private static readonly object _cacheLock = new();

    private static readonly string[] CommonSteamPaths =
    [
        @"C:\Program Files (x86)\Steam",
        @"C:\Program Files\Steam"
    ];

    private const string LibraryFoldersVdfRelativePath = @"steamapps\libraryfolders.vdf";

    /// <summary>
    /// Synchronously locates the Steam library folders VDF file path.
    /// Uses an in-memory cache to avoid repeated searches.
    /// </summary>
    /// <returns>
    /// A <see cref="SteamVDFLocation"/> containing either the found file path or an error description.
    /// </returns>
    public static SteamVDFLocation GetSteamVDFPath()
    {
        lock (_cacheLock)
        {
            if (_cachedResult.HasValue)
            {
                return _cachedResult.Value;
            }

            _cachedResult = LocateVDFPath();
            return _cachedResult.Value;
        }
    }

    /// <summary>
    /// Asynchronously locates the Steam library folders VDF file path.
    /// Uses an in-memory cache to avoid repeated searches.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation, containing a <see cref="SteamVDFLocation"/>
    /// with either the found file path or an error description.
    /// </returns>
    public static Task<SteamVDFLocation> GetSteamVDFPathAsync()
    {
        lock (_cacheLock)
        {
            if (_cachedResult.HasValue)
            {
                return Task.FromResult(_cachedResult.Value);
            }
        }

        return Task.Run(() =>
        {
            lock (_cacheLock)
            {
                if (_cachedResult.HasValue)
                {
                    return _cachedResult.Value;
                }

                _cachedResult = LocateVDFPath();
                return _cachedResult.Value;
            }
        });
    }

    private static SteamVDFLocation LocateVDFPath()
    {
        // Strategy 1: Check Windows Registry
        var registryResult = TryGetSteamPathFromRegistry();
        if (registryResult is not null)
        {
            var vdfPath = Path.Combine(registryResult, LibraryFoldersVdfRelativePath);
            if (File.Exists(vdfPath))
            {
                return new SteamVDFLocation(vdfPath);
            }
        }

        // Strategy 2: Check common installation paths
        foreach (var basePath in CommonSteamPaths)
        {
            var vdfPath = Path.Combine(basePath, LibraryFoldersVdfRelativePath);
            if (File.Exists(vdfPath))
            {
                return new SteamVDFLocation(vdfPath);
            }
        }

        // Not found
        return new SteamVDFLocation(new SteamVDFNotFoundError(
            "Steam library folders VDF file not found. " +
            "Ensure Steam is installed and the libraryfolders.vdf file exists."));
    }

    private static string? TryGetSteamPathFromRegistry()
    {
        try
        {
            // Try HKEY_CURRENT_USER first (user-specific installation)
            var path = Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Valve\Steam",
                "SteamPath",
                null) as string;

            if (!string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            // Try HKEY_LOCAL_MACHINE (system-wide installation)
            path = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\Software\Valve\Steam",
                "InstallPath",
                null) as string;

            if (!string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            // Try 32-bit registry on 64-bit Windows
            path = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\Software\WOW6432Node\Valve\Steam",
                "InstallPath",
                null) as string;

            return string.IsNullOrWhiteSpace(path) ? null : path;
        }
        catch (Exception)
        {
            // Registry access denied or key doesn't exist
            return null;
        }
    }
}

/// <summary>
/// Represents the result of attempting to locate the Steam library folders VDF file.
/// Contains either a successful file path or an error.
/// </summary>
[SupportedOSPlatform("windows")]
public readonly struct SteamVDFLocation : IEquatable<SteamVDFLocation>
{
    private readonly OneOf<string, SteamVDFNotFoundError> _value;

    /// <summary>
    /// Creates a successful result with the VDF file path.
    /// </summary>
    /// <param name="path">The path to the libraryfolders.vdf file.</param>
    public SteamVDFLocation(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _value = path;
    }

    /// <summary>
    /// Creates an error result.
    /// </summary>
    /// <param name="error">The error describing why the VDF file could not be located.</param>
    public SteamVDFLocation(SteamVDFNotFoundError error)
    {
        _value = error;
    }

    /// <summary>
    /// Indicates whether the VDF file was successfully located.
    /// </summary>
    public bool IsSuccess => _value.IsT0;

    /// <summary>
    /// Indicates whether an error occurred during location.
    /// </summary>
    public bool IsError => _value.IsT1;

    /// <summary>
    /// Gets the VDF file path if successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Path on an error result.</exception>
    public string Path => _value.IsT0
        ? _value.AsT0
        : throw new InvalidOperationException("Cannot access Path on an error result. Check IsSuccess first.");

    /// <summary>
    /// Gets the error if the location failed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing Error on a success result.</exception>
    public SteamVDFNotFoundError Error => _value.IsT1
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
        Func<string, TResult> onSuccess,
        Func<SteamVDFNotFoundError, TResult> onError)
    {
        return _value.Match(onSuccess, onError);
    }

    /// <summary>
    /// Executes one of two actions depending on whether the result is success or error.
    /// </summary>
    /// <param name="onSuccess">Action to execute if the result is successful.</param>
    /// <param name="onError">Action to execute if the result is an error.</param>
    public void Switch(
        Action<string> onSuccess,
        Action<SteamVDFNotFoundError> onError)
    {
        _value.Switch(onSuccess, onError);
    }

    /// <summary>
    /// Attempts to get the path value.
    /// </summary>
    /// <param name="path">The path if successful; otherwise, null.</param>
    /// <returns>True if successful; otherwise, false.</returns>
    public bool TryGetPath(out string? path)
    {
        if (_value.IsT0)
        {
            path = _value.AsT0;
            return true;
        }

        path = null;
        return false;
    }

    /// <summary>
    /// Attempts to get the error value.
    /// </summary>
    /// <param name="error">The error if failed; otherwise, default.</param>
    /// <returns>True if failed; otherwise, false.</returns>
    public bool TryGetError(out SteamVDFNotFoundError error)
    {
        if (_value.IsT1)
        {
            error = _value.AsT1;
            return true;
        }

        error = default;
        return false;
    }

    public bool Equals(SteamVDFLocation other) => _value.Equals(other._value);
    public override bool Equals(object? obj) => obj is SteamVDFLocation other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(SteamVDFLocation left, SteamVDFLocation right) => left.Equals(right);
    public static bool operator !=(SteamVDFLocation left, SteamVDFLocation right) => !left.Equals(right);

    public override string ToString() => _value.Match(
        path => $"Success: {path}",
        error => $"Error: {error.Message}");
}

/// <summary>
/// Represents an error that occurred while trying to locate the Steam VDF file.
/// </summary>
[SupportedOSPlatform("windows")]
public readonly record struct SteamVDFNotFoundError(string Message)
{
    /// <summary>
    /// Gets the error message describing why the VDF file could not be located.
    /// </summary>
    public string Message { get; init; } = Message;
}
