using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;

namespace GameasDat.Core.Telemetry.Sources
{
    public class MemoryMappedFileSource<T> : TelemetrySourceBase<T> where T : unmanaged
    {
        private readonly string _mapName;
        private readonly TimeSpan _pollInterval;
        private MemoryMappedFile? _mmf;
        private MemoryMappedViewAccessor? _accessor;

        public MemoryMappedFileSource(string mapName, TimeSpan pollInterval)
        {
            _mapName = mapName;
            _pollInterval = pollInterval;
        }

        public override async IAsyncEnumerable<T> ReadContinuousAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            // Open the memory-mapped file
            _mmf = MemoryMappedFile.OpenExisting(_mapName, MemoryMappedFileRights.Read);
            _accessor = _mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);

            try
            {
                while (!ct.IsCancellationRequested)
                {
                    T data;
                    _accessor.Read(0, out data);

                    yield return data;

                    await Task.Delay(_pollInterval, ct);
                }
            }
            finally
            {
                _accessor?.Dispose();
                _mmf?.Dispose();
            }
        }

        public override void Dispose()
        {
            _accessor?.Dispose();
            _mmf?.Dispose();
        }
    }
}
