using GameasDat.Core.Reader;
using GameasDat.Core.Telemetry.Sources.AssettoCorsa;
using K4os.Compression.LZ4.Streams;
using System.Runtime.InteropServices;
using Xunit;

namespace GamesDat.Tests;

/// <summary>
/// Tests to ensure new header functionality doesn't break reading of legacy session files
/// </summary>
public class BackwardCompatibilityTests : IDisposable
{
    private readonly string _testDirectory;

    public BackwardCompatibilityTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"gamesdat_compat_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    /// <summary>
    /// Creates a legacy session file WITHOUT a header (old format)
    /// </summary>
    private void CreateLegacySessionFile(string filePath, ACCPhysics[] frames, long[] timestamps)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create);
        using var lz4Stream = LZ4Stream.Encode(fileStream, leaveOpen: false);

        for (int i = 0; i < frames.Length; i++)
        {
            // Write timestamp
            lz4Stream.Write(BitConverter.GetBytes(timestamps[i]));

            // Write size
            int size = Marshal.SizeOf<ACCPhysics>();
            lz4Stream.Write(BitConverter.GetBytes(size));

            // Write struct data
            unsafe
            {
                var data = frames[i];
                byte* ptr = (byte*)&data;
                var span = new Span<byte>(ptr, size);
                lz4Stream.Write(span);
            }
        }
    }

    [Fact]
    public async Task ReadLegacyFile_WithoutHeader_ReadsSuccessfully()
    {
        // Arrange - Create legacy file
        var filePath = Path.Combine(_testDirectory, "legacy.dat");
        var testFrames = new[]
        {
            new ACCPhysics { PacketId = 1, SpeedKmh = 100.0f, RPM = 5000 },
            new ACCPhysics { PacketId = 2, SpeedKmh = 150.0f, RPM = 6000 },
            new ACCPhysics { PacketId = 3, SpeedKmh = 200.0f, RPM = 7000 }
        };
        var timestamps = new[] { 1000L, 2000L, 3000L };

        CreateLegacySessionFile(filePath, testFrames, timestamps);

        // Act - Read with new reader
        var frames = new List<(long timestamp, ACCPhysics data)>();
        await foreach (var frame in SessionReader.ReadAsync<ACCPhysics>(filePath))
        {
            frames.Add(frame);
        }

        // Assert
        Assert.Equal(3, frames.Count);

        for (int i = 0; i < testFrames.Length; i++)
        {
            Assert.Equal(timestamps[i], frames[i].timestamp);
            Assert.Equal(testFrames[i].PacketId, frames[i].data.PacketId);
            Assert.Equal(testFrames[i].SpeedKmh, frames[i].data.SpeedKmh);
            Assert.Equal(testFrames[i].RPM, frames[i].data.RPM);
        }
    }

    [Fact]
    public async Task ReadLegacyFile_LogsWarningMessage()
    {
        // Arrange - Create legacy file
        var filePath = Path.Combine(_testDirectory, "legacy_with_warning.dat");
        var testFrame = new ACCPhysics { SpeedKmh = 100.0f };

        CreateLegacySessionFile(filePath, new[] { testFrame }, new[] { 1000L });

        // Capture console output
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        try
        {
            // Act
            await foreach (var _ in SessionReader.ReadAsync<ACCPhysics>(filePath))
            {
                break; // Read one frame
            }

            // Assert
            var output = stringWriter.ToString();
            Assert.Contains("WARNING: Reading legacy session file", output);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task ReadLegacyFile_ManyFrames_AllPreserved()
    {
        // Arrange - Create legacy file with many frames
        var filePath = Path.Combine(_testDirectory, "legacy_many.dat");
        const int frameCount = 1000;

        var frames = Enumerable.Range(0, frameCount)
            .Select(i => new ACCPhysics { PacketId = i, SpeedKmh = 100.0f + i })
            .ToArray();

        var timestamps = Enumerable.Range(0, frameCount)
            .Select(i => (long)(i * 10000))
            .ToArray();

        CreateLegacySessionFile(filePath, frames, timestamps);

        // Act
        var readFrames = new List<ACCPhysics>();
        await foreach (var (_, data) in SessionReader.ReadAsync<ACCPhysics>(filePath))
        {
            readFrames.Add(data);
        }

        // Assert
        Assert.Equal(frameCount, readFrames.Count);

        for (int i = 0; i < frameCount; i++)
        {
            Assert.Equal(i, readFrames[i].PacketId);
            Assert.Equal(100.0f + i, readFrames[i].SpeedKmh);
        }
    }

    [Fact]
    public async Task MixedFileTypes_BothReadCorrectly()
    {
        // Arrange - Create one legacy and one new file
        var legacyPath = Path.Combine(_testDirectory, "legacy_mixed.dat");
        var newPath = Path.Combine(_testDirectory, "new_mixed.dat");

        var testData = new ACCPhysics { PacketId = 42, SpeedKmh = 123.45f };

        // Create legacy file
        CreateLegacySessionFile(legacyPath, new[] { testData }, new[] { 1000L });

        // Create new file with header
        var writer = new GameasDat.Core.Writer.BinarySessionWriter();
        writer.Start(newPath);
        writer.WriteFrame(testData, 1000L);
        writer.Stop();

        // Act & Assert - Both should read successfully
        ACCPhysics legacyData = default;
        await foreach (var (_, data) in SessionReader.ReadAsync<ACCPhysics>(legacyPath))
        {
            legacyData = data;
        }

        ACCPhysics newData = default;
        await foreach (var (_, data) in SessionReader.ReadAsync<ACCPhysics>(newPath))
        {
            newData = data;
        }

        // Both should have same data
        Assert.Equal(testData.PacketId, legacyData.PacketId);
        Assert.Equal(testData.SpeedKmh, legacyData.SpeedKmh);
        Assert.Equal(testData.PacketId, newData.PacketId);
        Assert.Equal(testData.SpeedKmh, newData.SpeedKmh);
    }

    [Fact]
    public async Task LegacyFile_DifferentStructTypes_WorkCorrectly()
    {
        // Test legacy format with ACCGraphics
        var filePath = Path.Combine(_testDirectory, "legacy_graphics.dat");

        var testFrame = new ACCGraphics
        {
            PacketId = 10,
            Status = 2,
            CompletedLaps = 5
        };

        // Create legacy file for Graphics
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        using (var lz4Stream = LZ4Stream.Encode(fileStream, leaveOpen: false))
        {
            lz4Stream.Write(BitConverter.GetBytes(1000L)); // timestamp
            int size = Marshal.SizeOf<ACCGraphics>();
            lz4Stream.Write(BitConverter.GetBytes(size));

            unsafe
            {
                byte* ptr = (byte*)&testFrame;
                var span = new Span<byte>(ptr, size);
                lz4Stream.Write(span);
            }
        }

        // Act - Read
        ACCGraphics readData = default;
        await foreach (var (_, data) in SessionReader.ReadAsync<ACCGraphics>(filePath))
        {
            readData = data;
        }

        // Assert
        Assert.Equal(testFrame.PacketId, readData.PacketId);
        Assert.Equal(testFrame.Status, readData.Status);
        Assert.Equal(testFrame.CompletedLaps, readData.CompletedLaps);
    }

    [Fact]
    public void LegacyFile_NoMagicBytes_DetectedCorrectly()
    {
        // Arrange - Create file starting with something other than "GDT\0"
        var filePath = Path.Combine(_testDirectory, "no_magic.dat");

        using (var fs = File.Create(filePath))
        using (var lz4 = LZ4Stream.Encode(fs))
        {
            // Legacy files start directly with LZ4-compressed frame data
            // LZ4 magic number is typically 0x184D2204, not "GDT\0"
            lz4.Write(BitConverter.GetBytes(DateTime.UtcNow.Ticks));
            lz4.Write(BitConverter.GetBytes(Marshal.SizeOf<ACCPhysics>()));

            var testData = new ACCPhysics { PacketId = 1 };
            unsafe
            {
                byte* ptr = (byte*)&testData;
                var span = new Span<byte>(ptr, Marshal.SizeOf<ACCPhysics>());
                lz4.Write(span);
            }
        }

        // Act - Read raw bytes to verify no magic bytes
        using var readStream = File.OpenRead(filePath);
        var firstBytes = new byte[4];
        readStream.Read(firstBytes, 0, 4);

        // Assert - Should NOT start with "GDT\0"
        bool hasGDTMagic = firstBytes[0] == 'G' && firstBytes[1] == 'D' &&
                          firstBytes[2] == 'T' && firstBytes[3] == 0;
        Assert.False(hasGDTMagic);
    }
}
