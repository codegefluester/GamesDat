using K4os.Compression.LZ4.Streams;
using MessagePack;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GameasDat.Core.Reader
{
    public static class SessionReader
    {
        public static async IAsyncEnumerable<(long timestamp, T data)> ReadAsync<T>(
            string filePath,
            [EnumeratorCancellation] CancellationToken ct = default) where T : unmanaged
        {
            await using var fileStream = File.OpenRead(filePath);
            await using var stream = LZ4Stream.Decode(fileStream, leaveOpen: false);

            int expectedSize = Marshal.SizeOf<T>();
            int frameNumber = 0;

            while (!ct.IsCancellationRequested)
            {
                long timestamp;
                int size;
                byte[] buffer;

                try
                {
                    // Read timestamp (8 bytes)
                    byte[] timestampBytes = new byte[8];
                    int read = await ReadExactAsync(stream, timestampBytes, 0, 8, ct);

                    if (read == 0)
                    {
                        // Clean end of stream
                        if (frameNumber > 0)
                            Console.WriteLine($"Successfully read {frameNumber} complete frames");
                        yield break;
                    }

                    if (read < 8)
                    {
                        // Partial frame at end
                        Console.WriteLine($"Note: Skipped incomplete frame at end (partial timestamp: {read}/8 bytes)");
                        Console.WriteLine($"Total complete frames: {frameNumber}");
                        yield break;
                    }

                    timestamp = BitConverter.ToInt64(timestampBytes);

                    // Read size (4 bytes)
                    byte[] sizeBytes = new byte[4];
                    read = await ReadExactAsync(stream, sizeBytes, 0, 4, ct);

                    if (read < 4)
                    {
                        Console.WriteLine($"Note: Skipped incomplete frame at end (partial size: {read}/4 bytes)");
                        Console.WriteLine($"Total complete frames: {frameNumber}");
                        yield break;
                    }

                    size = BitConverter.ToInt32(sizeBytes);

                    // Validate size
                    if (size != expectedSize)
                    {
                        Console.WriteLine($"WARNING: Frame {frameNumber} size mismatch. Expected {expectedSize}, got {size}");
                        Console.WriteLine($"This might indicate file corruption. Stopping read.");
                        Console.WriteLine($"Total frames read: {frameNumber}");
                        yield break;
                    }

                    // Read struct data
                    buffer = new byte[size];
                    read = await ReadExactAsync(stream, buffer, 0, size, ct);

                    if (read < size)
                    {
                        Console.WriteLine($"Note: Skipped incomplete frame at end (partial data: {read}/{size} bytes)");
                        Console.WriteLine($"Total complete frames: {frameNumber}");
                        yield break;
                    }
                }
                catch (EndOfStreamException)
                {
                    // LZ4 stream was cut off mid-block during capture (expected on Ctrl+C)
                    Console.WriteLine($"Note: Stream ended mid-compression-block (expected during emergency stop)");
                    Console.WriteLine($"Total complete frames recovered: {frameNumber}");
                    yield break;
                }
                catch (Exception ex) when (ex.Message.Contains("Data might be corrupted"))
                {
                    // LZ4 detected corrupted data
                    Console.WriteLine($"Note: Compression stream corrupted at end (expected during emergency stop)");
                    Console.WriteLine($"Total complete frames recovered: {frameNumber}");
                    yield break;
                }

                // Convert to struct
                T data;
                unsafe
                {
                    fixed (byte* ptr = buffer)
                    {
                        data = Marshal.PtrToStructure<T>((IntPtr)ptr);
                    }
                }

                frameNumber++;
                yield return (timestamp, data);
            }
        }

        /// <summary>
        /// Read exact number of bytes, or return how many were successfully read (0 if EOF, partial if stream ended)
        /// </summary>
        private static async Task<int> ReadExactAsync(Stream stream, byte[] buffer, int offset, int count, CancellationToken ct)
        {
            int totalRead = 0;
            while (totalRead < count)
            {
                int read = await stream.ReadAsync(buffer.AsMemory(offset + totalRead, count - totalRead), ct);
                if (read == 0)
                {
                    // Stream ended
                    return totalRead;
                }
                totalRead += read;
            }
            return totalRead;
        }
    }
}
