namespace GameasDat.Core.Telemetry
{
    public interface ITelemetrySource<T> : IDisposable
    {
        /// <summary>
        /// Continuously read telemetry data until cancellation
        /// </summary>
        IAsyncEnumerable<T> ReadContinuousAsync(CancellationToken ct = default);
    }
}
