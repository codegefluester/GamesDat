using GamesDat.Core.Writer;

namespace GamesDat.Core.Telemetry
{
    /// <summary>
    /// Abstract base class for telemetry sources providing common configuration functionality
    /// </summary>
    public abstract class TelemetrySourceBase<T> : ITelemetrySource<T>
    {
        /// <summary>
        /// Output file path. Use "auto" for auto-generation, null for realtime-only mode.
        /// </summary>
        public string? OutputPath { get; set; } = "auto";

        /// <summary>
        /// Custom writer for serializing telemetry data. If null, uses default BinarySessionWriter.
        /// </summary>
        public ISessionWriter? Writer { get; set; }

        /// <summary>
        /// Configure output file path
        /// </summary>
        public ITelemetrySource<T> OutputTo(string path)
        {
            OutputPath = path;
            return this;
        }

        /// <summary>
        /// Configure custom session writer
        /// </summary>
        public ITelemetrySource<T> UseWriter(ISessionWriter writer)
        {
            Writer = writer;
            return this;
        }

        /// <summary>
        /// Disable file output (realtime callbacks only)
        /// </summary>
        public ITelemetrySource<T> RealtimeOnly()
        {
            OutputPath = null;
            return this;
        }

        /// <summary>
        /// Reset to auto-generated output path
        /// </summary>
        public ITelemetrySource<T> AutoOutput()
        {
            OutputPath = "auto";
            return this;
        }

        /// <summary>
        /// Continuously read telemetry data until cancellation
        /// </summary>
        public abstract IAsyncEnumerable<T> ReadContinuousAsync(CancellationToken ct = default);

        /// <summary>
        /// Dispose resources used by the telemetry source
        /// </summary>
        public virtual void Dispose() { }
    }
}
