using GameasDat.Core.Reader;
using GameasDat.Core.Telemetry.Sources.AssettoCorsa;
using GameasDat.Core.Writer;
using Xunit;

namespace GamesDat.Tests;

public class SessionReaderWriterIntegrationTests : IDisposable
{
    private readonly string _testDirectory;

    public SessionReaderWriterIntegrationTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"gamesdat_tests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task WriteAndRead_SingleFrame_PreservesData()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "single_frame.dat");
        var writer = new BinarySessionWriter();
        var timestamp = DateTime.UtcNow.Ticks;

        var testData = new ACCPhysics
        {
            PacketId = 42,
            SpeedKmh = 123.45f,
            RPM = 7500,
            Gear = 4,
            Gas = 0.8f,
            Brake = 0.0f
        };

        // Act - Write
        writer.Start(filePath);
        writer.WriteFrame(testData, timestamp);
        writer.Stop();

        // Act - Read
        ACCPhysics readData = default;
        long readTimestamp = 0;
        int frameCount = 0;

        await foreach (var (ts, data) in SessionReader.ReadAsync<ACCPhysics>(filePath))
        {
            readTimestamp = ts;
            readData = data;
            frameCount++;
        }

        // Assert
        Assert.Equal(1, frameCount);
        Assert.Equal(timestamp, readTimestamp);
        Assert.Equal(testData.PacketId, readData.PacketId);
        Assert.Equal(testData.SpeedKmh, readData.SpeedKmh);
        Assert.Equal(testData.RPM, readData.RPM);
        Assert.Equal(testData.Gear, readData.Gear);
    }

    [Fact]
    public async Task WriteAndRead_MultipleFrames_PreservesAllData()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "multiple_frames.dat");
        var writer = new BinarySessionWriter();
        const int frameCount = 100;

        // Act - Write
        writer.Start(filePath);
        for (int i = 0; i < frameCount; i++)
        {
            var data = new ACCPhysics
            {
                PacketId = i,
                SpeedKmh = 100.0f + i,
                RPM = 5000 + (i * 10),
                Gear = (i % 6) + 1
            };
            writer.WriteFrame(data, DateTime.UtcNow.Ticks + i);
        }
        writer.Stop();

        // Act - Read
        var frames = new List<(long timestamp, ACCPhysics data)>();
        await foreach (var frame in SessionReader.ReadAsync<ACCPhysics>(filePath))
        {
            frames.Add(frame);
        }

        // Assert
        Assert.Equal(frameCount, frames.Count);

        for (int i = 0; i < frameCount; i++)
        {
            Assert.Equal(i, frames[i].data.PacketId);
            Assert.Equal(100.0f + i, frames[i].data.SpeedKmh);
            Assert.Equal(5000 + (i * 10), frames[i].data.RPM);
            Assert.Equal((i % 6) + 1, frames[i].data.Gear);
        }
    }

    [Fact]
    public async Task WriteAndRead_ACCGraphics_WorksCorrectly()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "graphics.dat");
        var writer = new BinarySessionWriter();

        var testData = new ACCGraphics
        {
            PacketId = 100,
            Status = 2,
            Session = 1,
            CompletedLaps = 5,
            Position = 3
        };

        // Act - Write
        writer.Start(filePath);
        writer.WriteFrame(testData, DateTime.UtcNow.Ticks);
        writer.Stop();

        // Act - Read
        ACCGraphics readData = default;
        await foreach (var (_, data) in SessionReader.ReadAsync<ACCGraphics>(filePath))
        {
            readData = data;
        }

        // Assert
        Assert.Equal(testData.PacketId, readData.PacketId);
        Assert.Equal(testData.Status, readData.Status);
        Assert.Equal(testData.CompletedLaps, readData.CompletedLaps);
        Assert.Equal(testData.Position, readData.Position);
    }

    [Fact]
    public async Task WriteAndRead_ACCCombinedData_WorksCorrectly()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "combined.dat");
        var writer = new BinarySessionWriter();

        var testData = new ACCCombinedData
        {
            Physics = new ACCPhysics { SpeedKmh = 200.5f, RPM = 8000 },
            Graphics = new ACCGraphics { CompletedLaps = 10, Position = 2 },
            PhysicsTimestamp = 1000,
            GraphicsTimestamp = 2000
        };

        // Act - Write
        writer.Start(filePath);
        writer.WriteFrame(testData, DateTime.UtcNow.Ticks);
        writer.Stop();

        // Act - Read
        ACCCombinedData readData = default;
        await foreach (var (_, data) in SessionReader.ReadAsync<ACCCombinedData>(filePath))
        {
            readData = data;
        }

        // Assert
        Assert.Equal(testData.Physics.SpeedKmh, readData.Physics.SpeedKmh);
        Assert.Equal(testData.Physics.RPM, readData.Physics.RPM);
        Assert.Equal(testData.Graphics.CompletedLaps, readData.Graphics.CompletedLaps);
        Assert.Equal(testData.PhysicsTimestamp, readData.PhysicsTimestamp);
    }

    [Fact]
    public async Task Read_WrongType_ThrowsInvalidOperationException()
    {
        // Arrange - Write ACCPhysics data
        var filePath = Path.Combine(_testDirectory, "physics_data.dat");
        var writer = new BinarySessionWriter();

        writer.Start(filePath);
        writer.WriteFrame(new ACCPhysics { SpeedKmh = 150.0f }, DateTime.UtcNow.Ticks);
        writer.Stop();

        // Act & Assert - Try to read as ACCGraphics
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in SessionReader.ReadAsync<ACCGraphics>(filePath))
            {
                break;
            }
        });

        Assert.Contains("Type mismatch", ex.Message);
    }

    [Fact]
    public void FileHeader_IncludesMetadata()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "with_header.dat");
        var writer = new BinarySessionWriter();

        // Act - Write one frame to trigger header write
        writer.Start(filePath);
        writer.WriteFrame(new ACCPhysics(), DateTime.UtcNow.Ticks);
        writer.Stop();

        // Assert - Check file starts with magic bytes
        using var fs = File.OpenRead(filePath);
        var magicBytes = new byte[4];
        fs.Read(magicBytes, 0, 4);

        Assert.Equal((byte)'G', magicBytes[0]);
        Assert.Equal((byte)'D', magicBytes[1]);
        Assert.Equal((byte)'T', magicBytes[2]);
        Assert.Equal(0, magicBytes[3]);
    }

    [Fact]
    public async Task LargeSession_ThousandsOfFrames_WorksCorrectly()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "large_session.dat");
        var writer = new BinarySessionWriter();
        const int frameCount = 10000;

        // Act - Write
        writer.Start(filePath);
        for (int i = 0; i < frameCount; i++)
        {
            var data = new ACCPhysics
            {
                PacketId = i,
                SpeedKmh = (float)(Math.Sin(i * 0.01) * 100 + 150) // Simulate speed variation
            };
            writer.WriteFrame(data, DateTime.UtcNow.Ticks + i);
        }
        writer.Stop();

        // Act - Read
        int readCount = 0;
        await foreach (var (_, data) in SessionReader.ReadAsync<ACCPhysics>(filePath))
        {
            readCount++;
            Assert.Equal(readCount - 1, data.PacketId);
        }

        // Assert
        Assert.Equal(frameCount, readCount);
    }

    [Fact]
    public async Task EmptyFile_ReadsZeroFrames()
    {
        // Arrange - Create empty file
        var filePath = Path.Combine(_testDirectory, "empty.dat");
        File.WriteAllBytes(filePath, Array.Empty<byte>());

        // Act
        int frameCount = 0;
        await foreach (var _ in SessionReader.ReadAsync<ACCPhysics>(filePath))
        {
            frameCount++;
        }

        // Assert
        Assert.Equal(0, frameCount);
    }
}
