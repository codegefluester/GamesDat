using K4os.Compression.LZ4.Streams;
using System.Runtime.InteropServices;
using GamesDat.Core.Helpers;
using GamesDat.Core.Models;

namespace GamesDat.Core.Writer
{
    public class BinarySessionWriter : ISessionWriter
    {
        private FileStream? _fileStream;
        private LZ4EncoderStream? _compressionStream;
        private readonly object _writeLock = new object();
        private int _frameCount = 0;
        private bool _headerWritten = false;

        public void Start(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            _fileStream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read,  // Allow reading while writing
                bufferSize: 4096,
                useAsync: false);

            // Note: Compression stream created in WriteFrame after header is written
            _headerWritten = false;
        }

        public void WriteFrame<T>(T data, long timestamp) where T : unmanaged
        {
            if (_fileStream == null)
                throw new InvalidOperationException("Writer not started");

            lock (_writeLock)
            {
                // Write header on first frame
                if (!_headerWritten)
                {
                    WriteHeader<T>();
                    _headerWritten = true;
                }

                if (_compressionStream == null)
                    throw new InvalidOperationException("Compression stream not initialized");

                // Write timestamp (8 bytes)
                Span<byte> timestampBytes = stackalloc byte[8];
                BitConverter.TryWriteBytes(timestampBytes, timestamp);
                _compressionStream.Write(timestampBytes);

                // Get struct size and write it (4 bytes)
                int size = Marshal.SizeOf<T>();
                Span<byte> sizeBytes = stackalloc byte[4];
                BitConverter.TryWriteBytes(sizeBytes, size);
                _compressionStream.Write(sizeBytes);

                // Write struct data
                unsafe
                {
                    byte* ptr = (byte*)&data;
                    Span<byte> dataSpan = new Span<byte>(ptr, size);
                    _compressionStream.Write(dataSpan);
                }

                _frameCount++;

                // Flush every 10 frames to disk (balance between performance and data safety)
                if (_frameCount % 10 == 0)
                {
                    _compressionStream.Flush();
                    _fileStream.Flush(flushToDisk: true);  // Force OS to write to disk
                }
            }
        }

        private void WriteHeader<T>() where T : unmanaged
        {
            if (_fileStream == null)
                throw new InvalidOperationException("File stream not initialized");

            // Extract metadata from type attributes
            var gameId = MetadataHelper.GetGameId<T>();
            var (major, minor, patch) = MetadataHelper.GetDataVersion<T>();
            var typeName = typeof(T).AssemblyQualifiedName!;
            var structSize = Marshal.SizeOf<T>();

            // Create header
            var header = new SessionFileHeader
            {
                GameId = gameId,
                TypeName = typeName,
                DataVersion = SessionFileHeader.EncodeVersion(major, minor, patch),
                StructSize = structSize
            };

            // Write header to base file stream (before compression)
            header.WriteToStream(_fileStream);

            // Log header details
            Console.WriteLine($"Writing session: Game={gameId}, Type={typeof(T).Name}, Version={major}.{minor}.{patch}");

            // Now create compression stream for data frames
            _compressionStream = LZ4Stream.Encode(_fileStream, leaveOpen: false);
        }

        public void Stop()
        {
            lock (_writeLock)
            {
                _compressionStream?.Flush();
                _fileStream?.Flush(flushToDisk: true);
                _compressionStream?.Dispose();
                _compressionStream = null;

                _fileStream?.Dispose();
                _fileStream = null;
            }

            Console.WriteLine($"Wrote {_frameCount} frames to file");
        }

        public void Dispose() => Stop();
    }
}