using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GamesDat.Core.Telemetry.Sources;

namespace GamesDat.Tests.Helpers;

/// <summary>
/// Provides reflection-based discovery of FileWatcherSourceBase implementations.
/// Automatically finds all game-specific file watcher sources for parameterized testing.
/// </summary>
public static class FileWatcherSourceDiscovery
{
    private static Type[]? _cachedSources;
    private static readonly object _lock = new();

    /// <summary>
    /// Discovers all concrete FileWatcherSourceBase subclasses in the GamesDat.Core assembly.
    /// Results are cached for performance.
    /// </summary>
    /// <returns>Array of concrete file watcher source types.</returns>
    public static Type[] DiscoverAllSources()
    {
        if (_cachedSources != null)
            return _cachedSources;

        lock (_lock)
        {
            if (_cachedSources != null)
                return _cachedSources;

            var assembly = typeof(FileWatcherSourceBase).Assembly;
            var baseType = typeof(FileWatcherSourceBase);

            _cachedSources = assembly.GetTypes()
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    type.IsAssignableTo(baseType) &&
                    type != typeof(FileWatcherSource) && // Exclude generic base
                    HasCompatibleConstructor(type))
                .OrderBy(type => type.Name)
                .ToArray();

            return _cachedSources;
        }
    }

    /// <summary>
    /// Extracts the file patterns that a file watcher source is configured to monitor.
    /// </summary>
    /// <param name="sourceType">The file watcher source type.</param>
    /// <returns>Array of file patterns (e.g., ["*.replay", "*.dem"]).</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve patterns for the source type.</exception>
    public static string[] GetExpectedPatterns(Type sourceType)
    {
        var options = GetDefaultOptions(sourceType);

        if (options == null)
        {
            throw new InvalidOperationException(
                $"Unable to retrieve default options for {sourceType.Name}. " +
                $"Ensure the type has a compatible ApplyDefaults method.");
        }

        return options.Patterns ?? Array.Empty<string>();
    }

    /// <summary>
    /// Determines if a file watcher source is configured to include subdirectories.
    /// </summary>
    /// <param name="sourceType">The file watcher source type.</param>
    /// <returns>True if subdirectories are monitored, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to retrieve options for the source type.</exception>
    public static bool GetIncludeSubdirectories(Type sourceType)
    {
        var options = GetDefaultOptions(sourceType);

        if (options == null)
        {
            throw new InvalidOperationException(
                $"Unable to retrieve default options for {sourceType.Name}. " +
                $"Ensure the type has a compatible ApplyDefaults method.");
        }

        return options.IncludeSubdirectories;
    }

    /// <summary>
    /// Gets the default FileWatcherOptions for a source type by calling its ApplyDefaults method.
    /// Supports both ApplyDefaults(FileWatcherOptions) and ApplyDefaults(string?) patterns.
    /// </summary>
    /// <param name="sourceType">The file watcher source type.</param>
    /// <returns>The default FileWatcherOptions, or null if unable to retrieve.</returns>
    private static FileWatcherOptions? GetDefaultOptions(Type sourceType)
    {
        // Try ApplyDefaults(FileWatcherOptions) pattern first (used by most sources)
        var optionsMethod = sourceType.GetMethod(
            "ApplyDefaults",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new[] { typeof(FileWatcherOptions) },
            null);

        if (optionsMethod != null)
        {
            // Call ApplyDefaults with a test options object
            var testOptions = new FileWatcherOptions { Path = Path.GetTempPath() };
            var result = optionsMethod.Invoke(null, new object[] { testOptions });
            return result as FileWatcherOptions;
        }

        // Fall back to ApplyDefaults(string?) pattern
        var stringMethod = sourceType.GetMethod(
            "ApplyDefaults",
            BindingFlags.NonPublic | BindingFlags.Static,
            null,
            new[] { typeof(string) },
            null);

        if (stringMethod != null)
        {
            // Call ApplyDefaults with a test path
            var result = stringMethod.Invoke(null, new object?[] { Path.GetTempPath() });
            return result as FileWatcherOptions;
        }

        return null;
    }

    /// <summary>
    /// Checks if a type has a constructor compatible with testing.
    /// Supports: FileWatcherOptions, string (optional), or parameterless constructors.
    /// </summary>
    private static bool HasCompatibleConstructor(Type type)
    {
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

        return constructors.Any(ctor =>
        {
            var parameters = ctor.GetParameters();

            // Parameterless constructor
            if (parameters.Length == 0)
                return true;

            // Constructor with FileWatcherOptions
            if (parameters.Length >= 1 && parameters[0].ParameterType == typeof(FileWatcherOptions))
                return true;

            // Constructor with optional string parameter (string? customPath = null)
            if (parameters.Length >= 1 &&
                parameters[0].ParameterType == typeof(string) &&
                parameters[0].IsOptional)
                return true;

            return false;
        });
    }

    /// <summary>
    /// Instantiates a file watcher source with a custom test path.
    /// </summary>
    /// <param name="sourceType">The file watcher source type to instantiate.</param>
    /// <param name="testPath">The test directory path to monitor.</param>
    /// <returns>Instantiated file watcher source, or null if instantiation fails.</returns>
    public static FileWatcherSourceBase? InstantiateSource(Type sourceType, string testPath)
    {
        try
        {
            // Try constructor with string parameter first
            var stringConstructor = sourceType.GetConstructors()
                .FirstOrDefault(c =>
                {
                    var parameters = c.GetParameters();
                    return parameters.Length >= 1 &&
                           parameters[0].ParameterType == typeof(string) &&
                           parameters[0].IsOptional;
                });

            if (stringConstructor != null)
            {
                // Pass testPath and default values for any additional optional parameters
                var parameters = stringConstructor.GetParameters();
                var args = new object?[parameters.Length];
                args[0] = testPath;
                for (int i = 1; i < parameters.Length; i++)
                {
                    args[i] = parameters[i].DefaultValue;
                }
                return (FileWatcherSourceBase)stringConstructor.Invoke(args);
            }

            // Try constructor with FileWatcherOptions
            var optionsConstructor = sourceType.GetConstructors()
                .FirstOrDefault(c =>
                {
                    var parameters = c.GetParameters();
                    return parameters.Length >= 1 && parameters[0].ParameterType == typeof(FileWatcherOptions);
                });

            if (optionsConstructor != null)
            {
                // Get default options and override path
                var defaultOptions = GetDefaultOptions(sourceType) ?? new FileWatcherOptions();
                defaultOptions.Path = testPath;
                return (FileWatcherSourceBase)optionsConstructor.Invoke(new object[] { defaultOptions });
            }

            // Try parameterless constructor as fallback
            var parameterlessConstructor = sourceType.GetConstructor(Type.EmptyTypes);
            if (parameterlessConstructor != null)
            {
                return (FileWatcherSourceBase)parameterlessConstructor.Invoke(Array.Empty<object>());
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
