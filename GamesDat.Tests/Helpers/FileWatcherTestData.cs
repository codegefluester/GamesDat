using System;
using System.Collections.Generic;
using System.Linq;

namespace GamesDat.Tests.Helpers;

/// <summary>
/// Provides xUnit MemberData for parameterized file watcher source tests.
/// Automatically generates test cases for all discovered file watcher sources.
/// </summary>
public class FileWatcherTestData
{
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
            // Skip Tekken8 for now (uses wildcard pattern "*.*" with subdirectories - needs special handling)
            if (sourceType.Name == "Tekken8ReplayFileSource")
                continue;

            var patterns = FileWatcherSourceDiscovery.GetExpectedPatterns(sourceType);

            // Skip sources without valid patterns
            if (patterns.Length == 0)
                continue;

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
            // Skip Tekken8 for now (uses wildcard pattern "*.*" with subdirectories - needs special handling)
            if (sourceType.Name == "Tekken8ReplayFileSource")
                continue;

            var includeSubdirs = FileWatcherSourceDiscovery.GetIncludeSubdirectories(sourceType);

            // Only include sources with subdirectory support
            if (!includeSubdirs)
                continue;

            var patterns = FileWatcherSourceDiscovery.GetExpectedPatterns(sourceType);

            // Skip sources without valid patterns
            if (patterns.Length == 0)
                continue;

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

            // Skip sources without valid patterns
            if (patterns.Length == 0)
                continue;

            yield return new object[]
            {
                sourceType,
                patterns,
                includeSubdirs
            };
        }
    }
}
