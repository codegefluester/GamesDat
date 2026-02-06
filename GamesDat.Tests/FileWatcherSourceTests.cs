using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GamesDat.Core.Telemetry.Sources;
using GamesDat.Tests.Helpers;
using Xunit;

namespace GamesDat.Tests;

/// <summary>
/// Comprehensive tests for all FileWatcherSourceBase implementations.
/// Uses reflection to automatically discover and test all game-specific file watcher sources.
/// </summary>
public class FileWatcherSourceTests : IDisposable
{
    private readonly string _testRootDirectory;

    public FileWatcherSourceTests()
    {
        // Create isolated test directory with GUID to avoid conflicts
        _testRootDirectory = Path.Combine(
            Path.GetTempPath(),
            "GamesDat.Tests",
            $"FileWatcherTests_{Guid.NewGuid():N}");

        Directory.CreateDirectory(_testRootDirectory);
    }

    public void Dispose()
    {
        // Clean up test directory recursively
        if (Directory.Exists(_testRootDirectory))
        {
            try
            {
                Directory.Delete(_testRootDirectory, recursive: true);
            }
            catch
            {
                // Ignore cleanup failures (file locks, etc.)
            }
        }
    }

    #region Test Methods

    /// <summary>
    /// Tests that file watcher sources detect newly created files matching their patterns.
    /// </summary>
    [Theory]
    [MemberData(nameof(FileWatcherTestData.AllSources), MemberType = typeof(FileWatcherTestData))]
    public async Task FileCreation_MatchingPattern_DetectsFile(Type sourceType, string[] patterns)
    {
        // Arrange
        var testDir = CreateTestDirectory(sourceType.Name);
        var source = InstantiateSource(sourceType, testDir);
        Assert.NotNull(source);

        var detectedFiles = new List<string>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act - Start watching in background
        var watchTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var file in source.ReadContinuousAsync(cts.Token))
                {
                    detectedFiles.Add(file);
                    if (detectedFiles.Count >= 1)
                    {
                        cts.Cancel(); // Stop after detecting first file
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when we cancel
            }
        }, cts.Token);

        // Allow FileSystemWatcher to initialize
        await Task.Delay(500);

        // Create a file matching the first pattern
        var testFileName = $"test_{Guid.NewGuid():N}{patterns[0].Replace("*", "")}";
        var testFilePath = Path.Combine(testDir, testFileName);
        await File.WriteAllTextAsync(testFilePath, "test content");

        // Wait for detection with timeout
        try
        {
            await watchTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        Assert.NotEmpty(detectedFiles);
        Assert.Contains(testFilePath, detectedFiles);
    }

    /// <summary>
    /// Tests that file watcher sources ignore files that don't match their patterns.
    /// Skips sources with catch-all patterns like "*.*" which intentionally match all files.
    /// </summary>
    [Theory]
    [MemberData(nameof(FileWatcherTestData.AllSources), MemberType = typeof(FileWatcherTestData))]
    public async Task NonMatchingPattern_NotDetected(Type sourceType, string[] patterns)
    {
        // Skip sources with catch-all patterns (e.g., Tekken8 uses "*.*" because extension is unknown)
        if (patterns.Contains("*.*"))
        {
            return; // Skip test for catch-all patterns
        }

        // Arrange
        var testDir = CreateTestDirectory(sourceType.Name);
        var source = InstantiateSource(sourceType, testDir);
        Assert.NotNull(source);

        var detectedFiles = new List<string>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act - Start watching in background
        var watchTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var file in source.ReadContinuousAsync(cts.Token))
                {
                    detectedFiles.Add(file);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when timer expires
            }
        }, cts.Token);

        // Allow FileSystemWatcher to initialize
        await Task.Delay(500);

        // Create files with non-matching extensions
        var nonMatchingFiles = new[] { ".txt", ".log", ".tmp" };
        foreach (var extension in nonMatchingFiles)
        {
            var testFilePath = Path.Combine(testDir, $"test_{Guid.NewGuid():N}{extension}");
            await File.WriteAllTextAsync(testFilePath, "test content");
        }

        // Wait for potential detection (should timeout without detecting)
        await Task.Delay(2000);
        cts.Cancel();

        try
        {
            await watchTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert - No files should be detected
        Assert.Empty(detectedFiles);
    }

    /// <summary>
    /// Tests that file watcher sources discover files that existed before starting.
    /// </summary>
    [Theory]
    [MemberData(nameof(FileWatcherTestData.AllSources), MemberType = typeof(FileWatcherTestData))]
    public async Task ExistingFiles_OnStartup_AreDiscovered(Type sourceType, string[] patterns)
    {
        // Arrange
        var testDir = CreateTestDirectory(sourceType.Name);

        // Create file BEFORE starting watcher
        var testFileName = $"existing_{Guid.NewGuid():N}{patterns[0].Replace("*", "")}";
        var testFilePath = Path.Combine(testDir, testFileName);
        await File.WriteAllTextAsync(testFilePath, "existing file content");

        // Wait briefly to ensure file is written
        await Task.Delay(100);

        var source = InstantiateSource(sourceType, testDir);
        Assert.NotNull(source);

        var detectedFiles = new List<string>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act - Start watching (should discover existing file)
        var watchTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var file in source.ReadContinuousAsync(cts.Token))
                {
                    detectedFiles.Add(file);
                    if (detectedFiles.Count >= 1)
                    {
                        cts.Cancel();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }, cts.Token);

        // Wait for startup scan to complete
        try
        {
            await watchTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        Assert.NotEmpty(detectedFiles);
        Assert.Contains(testFilePath, detectedFiles);
    }

    /// <summary>
    /// Tests that sources with subdirectory support detect files in subdirectories.
    /// </summary>
    [Theory]
    [MemberData(nameof(FileWatcherTestData.SourcesWithSubdirectories), MemberType = typeof(FileWatcherTestData))]
    public async Task Subdirectories_WhenEnabled_DetectsFilesInSubdirs(Type sourceType, string[] patterns, bool includeSubdirs)
    {
        // Arrange
        var testDir = CreateTestDirectory(sourceType.Name);
        var subDir = Path.Combine(testDir, "SubFolder");
        Directory.CreateDirectory(subDir);

        var source = InstantiateSource(sourceType, testDir);
        Assert.NotNull(source);

        // Verify subdirectory support is enabled
        Assert.True(includeSubdirs);

        var detectedFiles = new List<string>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act - Start watching
        var watchTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var file in source.ReadContinuousAsync(cts.Token))
                {
                    detectedFiles.Add(file);
                    if (detectedFiles.Count >= 1)
                    {
                        cts.Cancel();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }, cts.Token);

        // Allow FileSystemWatcher to initialize
        await Task.Delay(500);

        // Create file in subdirectory
        var testFileName = $"subdir_{Guid.NewGuid():N}{patterns[0].Replace("*", "")}";
        var testFilePath = Path.Combine(subDir, testFileName);
        await File.WriteAllTextAsync(testFilePath, "subdirectory file content");

        // Wait for detection
        try
        {
            await watchTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert - File in subdirectory should be detected
        Assert.NotEmpty(detectedFiles);
        Assert.Contains(testFilePath, detectedFiles);
    }

    /// <summary>
    /// Tests that each file is emitted only once, even if multiple events occur.
    /// </summary>
    [Theory]
    [MemberData(nameof(FileWatcherTestData.AllSources), MemberType = typeof(FileWatcherTestData))]
    public async Task FileEvents_EmittedOnlyOnce(Type sourceType, string[] patterns)
    {
        // Arrange
        var testDir = CreateTestDirectory(sourceType.Name);
        var source = InstantiateSource(sourceType, testDir);
        Assert.NotNull(source);

        var detectedFiles = new List<string>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));

        // Act - Start watching
        var watchTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var file in source.ReadContinuousAsync(cts.Token))
                {
                    detectedFiles.Add(file);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }, cts.Token);

        // Allow FileSystemWatcher to initialize
        await Task.Delay(500);

        // Create file
        var testFileName = $"once_{Guid.NewGuid():N}{patterns[0].Replace("*", "")}";
        var testFilePath = Path.Combine(testDir, testFileName);
        await File.WriteAllTextAsync(testFilePath, "initial content");

        // Wait for initial detection
        await Task.Delay(2000);

        // Modify file multiple times (should not trigger additional emissions)
        for (int i = 0; i < 3; i++)
        {
            await File.AppendAllTextAsync(testFilePath, $"\nmodification {i}");
            await Task.Delay(100);
        }

        // Wait to ensure no additional detections
        await Task.Delay(3000);
        cts.Cancel();

        try
        {
            await watchTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert - File should appear exactly once
        var occurrences = detectedFiles.Count(f => f == testFilePath);
        Assert.Equal(1, occurrences);
    }

    /// <summary>
    /// Tests that rapid events for the same file are debounced.
    /// </summary>
    [Theory]
    [MemberData(nameof(FileWatcherTestData.AllSources), MemberType = typeof(FileWatcherTestData))]
    public async Task RapidEvents_SameFile_Debounced(Type sourceType, string[] patterns)
    {
        // Arrange
        var testDir = CreateTestDirectory(sourceType.Name);
        var source = InstantiateSource(sourceType, testDir);
        Assert.NotNull(source);

        var detectedFiles = new List<string>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        // Act - Start watching
        var watchTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var file in source.ReadContinuousAsync(cts.Token))
                {
                    detectedFiles.Add(file);
                    if (detectedFiles.Count >= 1)
                    {
                        // Give time for any duplicate events, then stop
                        await Task.Delay(3000);
                        cts.Cancel();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }, cts.Token);

        // Allow FileSystemWatcher to initialize
        await Task.Delay(500);

        // Create file with rapid modifications
        var testFileName = $"debounce_{Guid.NewGuid():N}{patterns[0].Replace("*", "")}";
        var testFilePath = Path.Combine(testDir, testFileName);

        // Rapid writes (within debounce window)
        await File.WriteAllTextAsync(testFilePath, "initial");
        await Task.Delay(50);
        await File.AppendAllTextAsync(testFilePath, " - rapid 1");
        await Task.Delay(50);
        await File.AppendAllTextAsync(testFilePath, " - rapid 2");

        // Wait for detection
        try
        {
            await watchTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert - Should be debounced to single emission
        var occurrences = detectedFiles.Count(f => f == testFilePath);
        Assert.Equal(1, occurrences);
    }

    /// <summary>
    /// Validates that all discovered file watcher sources can be instantiated.
    /// </summary>
    [Fact]
    public void AllDiscoveredSources_AreInstantiable()
    {
        // Arrange - Use AllSources() which includes pattern filtering
        var testData = FileWatcherTestData.AllSources().ToList();
        var testDir = CreateTestDirectory("InstantiationTest");

        // Act & Assert
        Assert.NotEmpty(testData); // Should discover file watcher source types with patterns

        foreach (var data in testData)
        {
            var sourceType = (Type)data[0];
            var patterns = (string[])data[1];

            var instance = FileWatcherSourceDiscovery.InstantiateSource(sourceType, testDir);

            Assert.NotNull(instance);
            Assert.IsAssignableFrom<FileWatcherSourceBase>(instance);
            Assert.NotEmpty(patterns);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates an isolated test directory for a specific test scenario.
    /// </summary>
    /// <param name="subFolder">Name of the subfolder (typically source type name).</param>
    /// <returns>Full path to the created test directory.</returns>
    private string CreateTestDirectory(string subFolder)
    {
        var testPath = Path.Combine(_testRootDirectory, subFolder, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(testPath);
        return testPath;
    }

    /// <summary>
    /// Instantiates a file watcher source with a custom test path.
    /// </summary>
    /// <param name="sourceType">The type of file watcher source to instantiate.</param>
    /// <param name="testPath">The test directory path to monitor.</param>
    /// <returns>Instantiated file watcher source.</returns>
    private FileWatcherSourceBase InstantiateSource(Type sourceType, string testPath)
    {
        var instance = FileWatcherSourceDiscovery.InstantiateSource(sourceType, testPath);
        Assert.NotNull(instance);
        return instance;
    }

    #endregion
}
