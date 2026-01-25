using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
