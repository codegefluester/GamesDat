namespace GameasDat.Core.Models;

/// <summary>
/// Represents the metadata header for session files.
/// This header is written before the LZ4-compressed data frames.
/// </summary>
public class SessionFileHeader
{
    /// <summary>
    /// Magic bytes to identify GamesDat session files: "GDT\0"
    /// </summary>
    public static readonly byte[] MagicBytes = [(byte)'G', (byte)'D', (byte)'T', 0];

    /// <summary>
    /// Current version of the header format. Increment when header structure changes.
    /// </summary>
    public const ushort HeaderVersion = 1;

    /// <summary>
    /// Identifier for the game that generated this session (e.g., "ACC")
    /// </summary>
    public required string GameId { get; init; }

    /// <summary>
    /// Assembly-qualified type name of the data structure stored in this file
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Version of the data structure encoded as: (major &lt;&lt; 24) | (minor &lt;&lt; 16) | patch
    /// </summary>
    public required uint DataVersion { get; init; }

    /// <summary>
    /// Size of the struct in bytes (from Marshal.SizeOf)
    /// </summary>
    public required int StructSize { get; init; }

    /// <summary>
    /// Encodes a semantic version into a 32-bit unsigned integer
    /// </summary>
    public static uint EncodeVersion(int major, int minor, int patch)
    {
        if (major < 0 || major > 255)
            throw new ArgumentOutOfRangeException(nameof(major), "Major version must be 0-255");
        if (minor < 0 || minor > 255)
            throw new ArgumentOutOfRangeException(nameof(minor), "Minor version must be 0-255");
        if (patch < 0 || patch > 65535)
            throw new ArgumentOutOfRangeException(nameof(patch), "Patch version must be 0-65535");

        return (uint)((major << 24) | (minor << 16) | patch);
    }

    /// <summary>
    /// Decodes a 32-bit unsigned integer into semantic version components
    /// </summary>
    public static (int major, int minor, int patch) DecodeVersion(uint encoded)
    {
        int major = (int)(encoded >> 24);
        int minor = (int)((encoded >> 16) & 0xFF);
        int patch = (int)(encoded & 0xFFFF);
        return (major, minor, patch);
    }

    /// <summary>
    /// Writes the header to a stream
    /// </summary>
    public void WriteToStream(Stream stream)
    {
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);

        // Write magic bytes (4 bytes)
        writer.Write(MagicBytes);

        // Write header version (2 bytes)
        writer.Write(HeaderVersion);

        // Write game ID (1 byte length + N bytes UTF-8)
        var gameIdBytes = System.Text.Encoding.UTF8.GetBytes(GameId);
        if (gameIdBytes.Length > 255)
            throw new InvalidOperationException($"Game ID too long: {gameIdBytes.Length} bytes (max 255)");
        writer.Write((byte)gameIdBytes.Length);
        writer.Write(gameIdBytes);

        // Write type name (2 bytes length + M bytes UTF-8)
        var typeNameBytes = System.Text.Encoding.UTF8.GetBytes(TypeName);
        if (typeNameBytes.Length > 65535)
            throw new InvalidOperationException($"Type name too long: {typeNameBytes.Length} bytes (max 65535)");
        writer.Write((ushort)typeNameBytes.Length);
        writer.Write(typeNameBytes);

        // Write data version (4 bytes)
        writer.Write(DataVersion);

        // Write struct size (4 bytes)
        writer.Write(StructSize);

        // Write reserved bytes (16 bytes of zeros)
        writer.Write(new byte[16]);
    }

    /// <summary>
    /// Reads a header from a stream
    /// </summary>
    public static SessionFileHeader ReadFromStream(Stream stream)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);

        // Read and validate magic bytes
        var magic = reader.ReadBytes(4);
        if (!magic.SequenceEqual(MagicBytes))
            throw new InvalidOperationException(
                $"Invalid magic bytes. Expected {BitConverter.ToString(MagicBytes)}, " +
                $"got {BitConverter.ToString(magic)}");

        // Read header version
        var headerVersion = reader.ReadUInt16();
        if (headerVersion > HeaderVersion)
            throw new InvalidOperationException(
                $"Unsupported header version {headerVersion}. This reader supports version {HeaderVersion}");

        // Read game ID
        var gameIdLength = reader.ReadByte();
        var gameIdBytes = reader.ReadBytes(gameIdLength);
        var gameId = System.Text.Encoding.UTF8.GetString(gameIdBytes);

        // Read type name
        var typeNameLength = reader.ReadUInt16();
        var typeNameBytes = reader.ReadBytes(typeNameLength);
        var typeName = System.Text.Encoding.UTF8.GetString(typeNameBytes);

        // Read data version
        var dataVersion = reader.ReadUInt32();

        // Read struct size
        var structSize = reader.ReadInt32();

        // Skip reserved bytes
        reader.ReadBytes(16);

        return new SessionFileHeader
        {
            GameId = gameId,
            TypeName = typeName,
            DataVersion = dataVersion,
            StructSize = structSize
        };
    }
}
