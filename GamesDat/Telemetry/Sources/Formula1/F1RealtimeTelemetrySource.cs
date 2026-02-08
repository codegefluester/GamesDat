using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    internal class F1RealtimeTelemetrySource : UdpSourceBase<F1TelemetryFrame>
    {
        public F1RealtimeTelemetrySource(UdpSourceOptions options) : base(options)
        {
        }

        protected override IEnumerable<F1TelemetryFrame> ProcessData(byte[] data)
        {
            yield return new F1TelemetryFrame();
        }
    }
}
