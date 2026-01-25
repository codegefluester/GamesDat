using K4os.Compression.LZ4.Streams;
using System.Runtime.InteropServices;

namespace GameasDat.Core.Writer
{
    public class BinarySessionWriter : ISessionWriter
    {
        private FileStream? _fileStream;
        private LZ4EncoderStream? _compressionStream;
        private readonly object _writeLock = new object();

        public void Start(string filePath)
        {
            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            _fileStream = File.Create(filePath);
            _compressionStream = LZ4Stream.Encode(_fileStream, leaveOpen: false);
        }

        public void WriteFrame<T>(T data, long timestamp) where T : unmanaged
        {
            if (_compressionStream == null)
                throw new InvalidOperationException("Writer not started");

            // Lock to ensure atomic writes (timestamp + size + data written together)
            lock (_writeLock)
            {
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

                // Flush periodically to minimize data loss on crash
                // (LZ4 buffers internally, so we flush every frame to be safe)
                _compressionStream.Flush();
            }
        }

        public void Stop()
        {
            lock (_writeLock)
            {
                _compressionStream?.Flush();
                _compressionStream?.Dispose();
                _compressionStream = null;

                _fileStream?.Dispose();
                _fileStream = null;
            }
        }

        public void Dispose() => Stop();
    }
}