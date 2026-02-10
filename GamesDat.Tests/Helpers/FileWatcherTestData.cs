using System;
using System.Collections.Generic;
using System.Linq;
using GamesDat.Core.Telemetry.Sources;
using GamesDat.Core.Telemetry.Sources.AgeOfEmpires4;
using GamesDat.Core.Telemetry.Sources.Tekken8;

namespace GamesDat.Tests.Helpers;

/// <summary>
/// Provides xUnit MemberData for parameterized file watcher source tests.
/// Automatically generates test cases for all discovered file watcher sources.
/// </summary>
public class FileWatcherTestData
{

    /// <summary>
    /// Sources we currently explicitly ignore during testing ebcause they use an
    /// overly broad pattern ("*.*") and require special handling to avoid test pollution.
    /// </summary>
    public static Type[] IgnoredSources = [
        typeof(Tekken8ReplayFileSource), 
        typeof(AgeOfEmpires4ReplayFileSource)
    ];

    /// <summary>
    /// Returns all discovered file watcher sources as test case data.
    /// Each test case includes: Type sourceType, string[] patterns
    /// </summary>
    /// <returns>Enumerable of test cases for all file watcher sources.</returns>
    public static IEnumerable<object[]> AllSources()
    {
        var sources = FileWatcherSourceDiscovery.DiscoverAllSources();

        foreach (var sourceType in sources)
        {
            if (IgnoredSources.Contains(sourceType))
            {
                Console.WriteLine($"Skipping source {sourceType.Name} in AllSources test data due to broad pattern and special handling requirements.");
                continue;
            }

            var patterns = FileWatcherSourceDiscovery.GetExpectedPatterns(sourceType);

            // Validate that patterns were successfully retrieved
            if (patterns.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Source type {sourceType.Name} has no file patterns defined. " +
                    $"This likely indicates a problem with the ApplyDefaults method or pattern discovery.");
            }

            yield return new object[]
            {
                sourceType,
                patterns
            };
        }
    }

    /// <summary>
    /// Returns file watcher sources that have subdirectory support enabled.
    /// Each test case includes: Type sourceType, string[] patterns, bool includeSubdirs
    /// </summary>
    /// <returns>Enumerable of test cases for sources with subdirectory support.</returns>
    public static IEnumerable<object[]> SourcesWithSubdirectories()
    {
        var sources = FileWatcherSourceDiscovery.DiscoverAllSources();

        foreach (var sourceType in sources)
        {
            // Skip Tekken8 (uses wildcard pattern "*.*" with subdirectories - needs special handling)
            if (sourceType == typeof(Tekken8ReplayFileSource))
                continue;

            var includeSubdirs = FileWatcherSourceDiscovery.GetIncludeSubdirectories(sourceType);

            // Only include sources with subdirectory support
            if (!includeSubdirs)
                continue;

            var patterns = FileWatcherSourceDiscovery.GetExpectedPatterns(sourceType);

            // Validate that patterns were successfully retrieved
            if (patterns.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Source type {sourceType.Name} has no file patterns defined. " +
                    $"This likely indicates a problem with the ApplyDefaults method or pattern discovery.");
            }

            yield return new object[]
            {
                sourceType,
                patterns,
                includeSubdirs
            };
        }
    }

    /// <summary>
    /// Returns file watcher sources without subdirectory support.
    /// Each test case includes: Type sourceType, string[] patterns, bool includeSubdirs
    /// </summary>
    /// <returns>Enumerable of test cases for sources without subdirectory support.</returns>
    public static IEnumerable<object[]> SourcesWithoutSubdirectories()
    {
        var sources = FileWatcherSourceDiscovery.DiscoverAllSources();

        foreach (var sourceType in sources)
        {
            var includeSubdirs = FileWatcherSourceDiscovery.GetIncludeSubdirectories(sourceType);

            // Only include sources without subdirectory support
            if (includeSubdirs)
                continue;

            var patterns = FileWatcherSourceDiscovery.GetExpectedPatterns(sourceType);

            // Validate that patterns were successfully retrieved
            if (patterns.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Source type {sourceType.Name} has no file patterns defined. " +
                    $"This likely indicates a problem with the ApplyDefaults method or pattern discovery.");
            }

            yield return new object[]
            {
                sourceType,
                patterns,
                includeSubdirs
            };
        }
    }
}
