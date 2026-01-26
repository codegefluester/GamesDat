using GamesDat.Core.Writer;

namespace GamesDat.Core.Telemetry
{
    public interface ITelemetrySource<T> : IDisposable
    {
        /// <summary>
        /// Output file path. Use "auto" for auto-generation, null for realtime-only mode.
        /// </summary>
        string? OutputPath { get; set; }

        /// <summary>
        /// Custom writer for serializing telemetry data. If null, uses default BinarySessionWriter.
        /// </summary>
        ISessionWriter? Writer { get; set; }

        /// <summary>
        /// Continuously read telemetry data until cancellation
        /// </summary>
        IAsyncEnumerable<T> ReadContinuousAsync(CancellationToken ct = default);

        /// <summary>
        /// Configure output file path
        /// </summary>
        ITelemetrySource<T> OutputTo(string path);

        /// <summary>
        /// Configure custom session writer
        /// </summary>
        ITelemetrySource<T> UseWriter(ISessionWriter writer);

        /// <summary>
        /// Disable file output (realtime callbacks only)
        /// </summary>
        ITelemetrySource<T> RealtimeOnly();

        /// <summary>
        /// Reset to auto-generated output path
        /// </summary>
        ITelemetrySource<T> AutoOutput();
    }
}
