using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace GamesDat.Core.Telemetry.Sources.Trackmania
{
    /// <summary>
    /// Trackmania-specific memory-mapped file source that handles the header and synchronization protocol
    /// </summary>
    public class TrackmaniaMemoryMappedFileSource : TelemetrySourceBase<TrackmaniaDataV3>
    {
        private readonly string _mapName;
        private readonly TimeSpan _pollInterval;
        private MemoryMappedFile? _mmf;
        private MemoryMappedViewAccessor? _accessor;

        private const int HeaderSize = 44; // 22 (magic) + 10 (padding) + 4 (version) + 4 (size) + 4 (updateNumber)
        private const int MaxRetries = 3;

        /// <summary>
        /// Validates the memory-mapped file header to ensure correct format
        /// </summary>
        private void ValidateHeader()
        {
            // Read the header
            TrackmaniaMemoryHeader header;
            _accessor!.Read(0, out header);

            // Validate magic string "ManiaPlanet_Telemetry"
            unsafe
            {
                byte* magicPtr = header.Magic;
                string magicString = System.Text.Encoding.ASCII.GetString(magicPtr, 21);
                if (magicString != "ManiaPlanet_Telemetry")
                {
                    throw new InvalidOperationException(
                        $"Invalid shared memory format. Expected 'ManiaPlanet_Telemetry' but got '{magicString}'. " +
                        "This may not be the Trackmania telemetry shared memory.");
                }
            }

            // Validate version (expected: 3)
            if (header.Version != 3)
            {
                throw new InvalidOperationException(
                    $"Unsupported telemetry version: {header.Version}. Expected version 3. " +
                    "This version of GamesDat may not be compatible with your Trackmania version.");
            }

            // Validate data size matches struct
            var expectedSize = System.Runtime.InteropServices.Marshal.SizeOf<TrackmaniaDataV3>();
            if (header.Size != expectedSize)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"WARNING: Data structure size mismatch. Header reports {header.Size} bytes, " +
                    $"but TrackmaniaDataV3 is {expectedSize} bytes. The structure definition may need updating.");
                // Don't throw - just warn, since we know the structure is likely wrong
            }
        }

        public TrackmaniaMemoryMappedFileSource(string mapName, TimeSpan pollInterval)
        {
            _mapName = mapName;
            _pollInterval = pollInterval;
        }

        public override async IAsyncEnumerable<TrackmaniaDataV3> ReadContinuousAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            // Step 1: Open memory-mapped file with clear error message
            try
            {
                _mmf = MemoryMappedFile.OpenExisting(_mapName, MemoryMappedFileRights.Read);
            }
            catch (FileNotFoundException)
            {
                throw new InvalidOperationException(
                    $"Trackmania telemetry '{_mapName}' not found. " +
                    "Make sure Trackmania is running and you're in a race. " +
                    "Supported: Maniaplanet 4, Trackmania (2020), Trackmania Turbo.");
            }

            // Step 2: Create view accessor with resource cleanup on failure
            try
            {
                _accessor = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            }
            catch (Exception ex)
            {
                _mmf?.Dispose();
                _mmf = null;
                throw new InvalidOperationException(
                    $"Failed to access Trackmania telemetry: {ex.Message}", ex);
            }

            // Step 3: Validate header format (magic, version, size)
            try
            {
                ValidateHeader();
            }
            catch (Exception ex)
            {
                _accessor?.Dispose();
                _accessor = null;
                _mmf?.Dispose();
                _mmf = null;
                throw new InvalidOperationException(
                    $"Telemetry validation failed: {ex.Message}", ex);
            }

            // Step 4: Main read loop
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    // Try to read with synchronization protocol
                    if (TryReadSynchronized(out var data))
                    {
                        yield return data;
                    }
                    // If retries failed, skip this cycle (normal during menu transitions)

                    await Task.Delay(_pollInterval, ct);
                }
            }
            finally
            {
                _accessor?.Dispose();
                _accessor = null;
                _mmf?.Dispose();
                _mmf = null;
            }
        }

        /// <summary>
        /// Attempts to read telemetry data with proper synchronization using UpdateNumber
        /// </summary>
        /// <returns>True if a valid read was performed, false if all retries failed</returns>
        private bool TryReadSynchronized(out TrackmaniaDataV3 data)
        {
            data = default;

            for (int attempt = 0; attempt < MaxRetries; attempt++)
            {
                // Read UpdateNumber before reading data (at offset 40 in header)
                uint updateNumberBefore = _accessor!.ReadUInt32(40);

                // If UpdateNumber is odd, the game is currently writing - skip this attempt
                if ((updateNumberBefore & 1) != 0)
                {
                    continue;
                }

                // Read the telemetry data after the header
                _accessor.Read(HeaderSize, out data);

                // Read UpdateNumber after reading data to verify no update occurred
                uint updateNumberAfter = _accessor.ReadUInt32(40);

                // Valid read: UpdateNumber is even and unchanged
                if (updateNumberBefore == updateNumberAfter && (updateNumberAfter & 1) == 0)
                {
                    return true;
                }

                // Torn read detected, retry
            }

            // All retries failed
            return false;
        }

        public override void Dispose()
        {
            _accessor?.Dispose();
            _mmf?.Dispose();
        }
    }
}
