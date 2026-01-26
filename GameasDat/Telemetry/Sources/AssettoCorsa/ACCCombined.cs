using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GameasDat.Core.Attributes;

namespace GameasDat.Core.Telemetry.Sources.AssettoCorsa
{
    /// <summary>
    /// Combined data structure containing all ACC telemetry streams.
    /// Physics updated at 100Hz, Graphics at 10Hz, Static at 5s intervals.
    /// </summary>
    [GameId("ACC")]
    [DataVersion(1, 0, 0)]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ACCCombinedData
    {
        // Core physics (updated every 10ms - 100Hz)
        public ACCPhysics Physics;

        // Session/race info (updated every 100ms - 10Hz)
        public ACCGraphics Graphics;

        // Static config (updated every 5 seconds)
        public ACCStatic Static;

        // Metadata for staleness detection
        public long PhysicsTimestamp;
        public long GraphicsTimestamp;
        public long StaticTimestamp;
    }

    /// <summary>
    /// High-performance combined source that aggregates Physics, Graphics, and Static data streams.
    /// Driven by Physics (100Hz) with cached Graphics/Static data to minimize latency.
    /// Eliminates BinarySessionWriter lock contention by using a single writer thread.
    /// </summary>
    public class ACCCombinedSource : TelemetrySourceBase<ACCCombinedData>
    {
        private readonly MemoryMappedFileSource<ACCPhysics> _physicsSource;
        private readonly MemoryMappedFileSource<ACCGraphics> _graphicsSource;
        private readonly MemoryMappedFileSource<ACCStatic> _staticSource;

        // Cached latest data from slower sources
        private ACCGraphics _latestGraphics;
        private ACCStatic _latestStatic;
        private long _graphicsTimestamp;
        private long _staticTimestamp;

        public ACCCombinedSource(
            MemoryMappedFileSource<ACCPhysics> physicsSource,
            MemoryMappedFileSource<ACCGraphics> graphicsSource,
            MemoryMappedFileSource<ACCStatic> staticSource)
        {
            _physicsSource = physicsSource ?? throw new ArgumentNullException(nameof(physicsSource));
            _graphicsSource = graphicsSource ?? throw new ArgumentNullException(nameof(graphicsSource));
            _staticSource = staticSource ?? throw new ArgumentNullException(nameof(staticSource));
        }

        /// <summary>
        /// Continuously read combined telemetry data at Physics rate (100Hz).
        /// Graphics and Static data are cached and updated in background tasks.
        /// </summary>
        public override async IAsyncEnumerable<ACCCombinedData> ReadContinuousAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            // Start background cache updaters for Graphics and Static
            var graphicsTask = UpdateGraphicsCacheAsync(ct);
            var staticTask = UpdateStaticCacheAsync(ct);

            try
            {
                // Main loop driven by Physics (100Hz)
                await foreach (var physics in _physicsSource.ReadContinuousAsync(ct))
                {
                    yield return new ACCCombinedData
                    {
                        Physics = physics,
                        Graphics = _latestGraphics,
                        Static = _latestStatic,
                        PhysicsTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        GraphicsTimestamp = _graphicsTimestamp,
                        StaticTimestamp = _staticTimestamp
                    };
                }
            }
            finally
            {
                // Ensure background tasks complete
                await Task.WhenAll(graphicsTask, staticTask);
            }
        }

        private async Task UpdateGraphicsCacheAsync(CancellationToken ct)
        {
            try
            {
                await foreach (var graphics in _graphicsSource.ReadContinuousAsync(ct))
                {
                    _latestGraphics = graphics;
                    _graphicsTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
        }

        private async Task UpdateStaticCacheAsync(CancellationToken ct)
        {
            try
            {
                await foreach (var staticData in _staticSource.ReadContinuousAsync(ct))
                {
                    _latestStatic = staticData;
                    _staticTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
        }

        public override void Dispose()
        {
            _physicsSource?.Dispose();
            _graphicsSource?.Dispose();
            _staticSource?.Dispose();
            base.Dispose();
        }
    }
}
