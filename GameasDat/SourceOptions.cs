using GameasDat.Core.Writer;

namespace GameasDat.Core
{
    /// <summary>
    /// Configuration options for a telemetry source
    /// </summary>
    public class SourceOptions
    {
        /// <summary>
        /// Path where session data will be written. 
        /// - "auto": Auto-generate filename based on source name and timestamp
        /// - null: No file output (real-time only)
        /// - string: Explicit file path
        /// </summary>
        public string? OutputPath { get; set; } = "auto";

        /// <summary>
        /// Custom session writer. If null, uses BinarySessionWriter
        /// </summary>
        public ISessionWriter? Writer { get; set; }

        /// <summary>
        /// Set explicit output path
        /// </summary>
        public SourceOptions OutputTo(string path)
        {
            OutputPath = path;
            return this;
        }

        /// <summary>
        /// Disable file output (real-time callbacks only)
        /// </summary>
        public SourceOptions RealtimeOnly()
        {
            OutputPath = null;
            return this;
        }

        /// <summary>
        /// Use a custom writer implementation
        /// </summary>
        public SourceOptions UseWriter(ISessionWriter writer)
        {
            Writer = writer;
            return this;
        }

        /// <summary>
        /// Auto-generate output filename (default behavior)
        /// </summary>
        public SourceOptions AutoOutput()
        {
            OutputPath = "auto";
            return this;
        }
    }
}
