using K4os.Compression.LZ4.Streams;
using System.Runtime.InteropServices;

namespace GameasDat.Core.Writer
{
    public class BinarySessionWriter : ISessionWriter
    {
        private FileStream? _fileStream;
        private LZ4EncoderStream? _compressionStream;
        private readonly object _writeLock = new object();
        private int _frameCount = 0;

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

            _compressionStream = LZ4Stream.Encode(_fileStream, leaveOpen: false);
        }

        public void WriteFrame<T>(T data, long timestamp) where T : unmanaged
        {
            if (_compressionStream == null || _fileStream == null)
                throw new InvalidOperationException("Writer not started");

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

                _frameCount++;

                // Flush every 10 frames to disk (balance between performance and data safety)
                if (_frameCount % 10 == 0)
                {
                    _compressionStream.Flush();
                    _fileStream.Flush(flushToDisk: true);  // Force OS to write to disk
                }
            }
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