using GameasDat.Core.Models;
using Xunit;

namespace GamesDat.Tests;

public class SessionFileHeaderTests
{
    [Fact]
    public void EncodeVersion_ValidValues_ReturnsCorrectEncoding()
    {
        // Test encoding version 1.2.3
        var encoded = SessionFileHeader.EncodeVersion(1, 2, 3);

        // Expected: (1 << 24) | (2 << 16) | 3 = 0x01020003
        Assert.Equal(0x01020003u, encoded);
    }

    [Fact]
    public void EncodeVersion_MaxValues_ReturnsCorrectEncoding()
    {
        // Test with maximum allowed values
        var encoded = SessionFileHeader.EncodeVersion(255, 255, 65535);

        // Expected: (255 << 24) | (255 << 16) | 65535 = 0xFFFFFFFF
        Assert.Equal(0xFFFFFFFFu, encoded);
    }

    [Theory]
    [InlineData(-1, 0, 0)]  // Negative major
    [InlineData(256, 0, 0)]  // Major > 255
    [InlineData(0, -1, 0)]  // Negative minor
    [InlineData(0, 256, 0)]  // Minor > 255
    [InlineData(0, 0, -1)]  // Negative patch
    [InlineData(0, 0, 65536)]  // Patch > 65535
    public void EncodeVersion_InvalidValues_ThrowsArgumentOutOfRangeException(int major, int minor, int patch)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SessionFileHeader.EncodeVersion(major, minor, patch));
    }

    [Fact]
    public void DecodeVersion_ValidEncoding_ReturnsCorrectValues()
    {
        var encoded = 0x01020003u; // Version 1.2.3

        var (major, minor, patch) = SessionFileHeader.DecodeVersion(encoded);

        Assert.Equal(1, major);
        Assert.Equal(2, minor);
        Assert.Equal(3, patch);
    }

    [Fact]
    public void DecodeVersion_MaxValues_ReturnsCorrectValues()
    {
        var encoded = 0xFFFFFFFFu;

        var (major, minor, patch) = SessionFileHeader.DecodeVersion(encoded);

        Assert.Equal(255, major);
        Assert.Equal(255, minor);
        Assert.Equal(65535, patch);
    }

    [Fact]
    public void EncodeDecodeVersion_RoundTrip_PreservesValues()
    {
        // Test round-trip encoding/decoding
        var originalMajor = 2;
        var originalMinor = 5;
        var originalPatch = 42;

        var encoded = SessionFileHeader.EncodeVersion(originalMajor, originalMinor, originalPatch);
        var (major, minor, patch) = SessionFileHeader.DecodeVersion(encoded);

        Assert.Equal(originalMajor, major);
        Assert.Equal(originalMinor, minor);
        Assert.Equal(originalPatch, patch);
    }

    [Fact]
    public void WriteToStream_ThenReadFromStream_RoundTrip_PreservesAllFields()
    {
        // Arrange
        var header = new SessionFileHeader
        {
            GameId = "ACC",
            TypeName = "GameasDat.Core.Test.TestStruct, GamesDat.Core",
            DataVersion = SessionFileHeader.EncodeVersion(1, 0, 0),
            StructSize = 1024
        };

        using var stream = new MemoryStream();

        // Act
        header.WriteToStream(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var readHeader = SessionFileHeader.ReadFromStream(stream);

        // Assert
        Assert.Equal(header.GameId, readHeader.GameId);
        Assert.Equal(header.TypeName, readHeader.TypeName);
        Assert.Equal(header.DataVersion, readHeader.DataVersion);
        Assert.Equal(header.StructSize, readHeader.StructSize);
    }

    [Fact]
    public void WriteToStream_IncludesMagicBytes()
    {
        var header = new SessionFileHeader
        {
            GameId = "TEST",
            TypeName = "Test.Type",
            DataVersion = SessionFileHeader.EncodeVersion(1, 0, 0),
            StructSize = 100
        };

        using var stream = new MemoryStream();
        header.WriteToStream(stream);

        stream.Seek(0, SeekOrigin.Begin);
        var magicBytes = new byte[4];
        stream.Read(magicBytes, 0, 4);

        Assert.Equal((byte)'G', magicBytes[0]);
        Assert.Equal((byte)'D', magicBytes[1]);
        Assert.Equal((byte)'T', magicBytes[2]);
        Assert.Equal(0, magicBytes[3]);
    }

    [Fact]
    public void ReadFromStream_InvalidMagicBytes_ThrowsInvalidOperationException()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Write invalid magic bytes
        writer.Write(new byte[] { (byte)'B', (byte)'A', (byte)'D', 0 });

        stream.Seek(0, SeekOrigin.Begin);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SessionFileHeader.ReadFromStream(stream));

        Assert.Contains("Invalid magic bytes", ex.Message);
    }

    [Fact]
    public void ReadFromStream_UnsupportedHeaderVersion_ThrowsInvalidOperationException()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        // Write magic bytes
        writer.Write(SessionFileHeader.MagicBytes);

        // Write unsupported header version (999)
        writer.Write((ushort)999);

        stream.Seek(0, SeekOrigin.Begin);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SessionFileHeader.ReadFromStream(stream));

        Assert.Contains("Unsupported header version", ex.Message);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("ACC")]
    [InlineData("iRacing")]
    [InlineData("Forza Motorsport")]
    public void WriteToStream_VariousGameIds_PreservesGameId(string gameId)
    {
        var header = new SessionFileHeader
        {
            GameId = gameId,
            TypeName = "Test.Type",
            DataVersion = SessionFileHeader.EncodeVersion(1, 0, 0),
            StructSize = 100
        };

        using var stream = new MemoryStream();
        header.WriteToStream(stream);

        stream.Seek(0, SeekOrigin.Begin);
        var readHeader = SessionFileHeader.ReadFromStream(stream);

        Assert.Equal(gameId, readHeader.GameId);
    }

    [Fact]
    public void WriteToStream_LongTypeName_PreservesTypeName()
    {
        var longTypeName = "Very.Long.Namespace.With.Many.Segments.MyCustomStruct, MyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

        var header = new SessionFileHeader
        {
            GameId = "TEST",
            TypeName = longTypeName,
            DataVersion = SessionFileHeader.EncodeVersion(1, 0, 0),
            StructSize = 500
        };

        using var stream = new MemoryStream();
        header.WriteToStream(stream);

        stream.Seek(0, SeekOrigin.Begin);
        var readHeader = SessionFileHeader.ReadFromStream(stream);

        Assert.Equal(longTypeName, readHeader.TypeName);
    }

    [Fact]
    public void WriteToStream_GameIdTooLong_ThrowsInvalidOperationException()
    {
        // Create a string longer than 255 bytes in UTF-8
        var longGameId = new string('X', 256);

        var header = new SessionFileHeader
        {
            GameId = longGameId,
            TypeName = "Test.Type",
            DataVersion = SessionFileHeader.EncodeVersion(1, 0, 0),
            StructSize = 100
        };

        using var stream = new MemoryStream();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            header.WriteToStream(stream));

        Assert.Contains("Game ID too long", ex.Message);
    }
}
