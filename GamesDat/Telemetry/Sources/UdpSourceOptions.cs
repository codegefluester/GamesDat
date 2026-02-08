using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesDat.Core.Telemetry.Sources
{
    public class UdpSourceOptions
    {
        /// <summary>
        /// Local port to listen for UDP packets
        /// </summary>
        public int Port { get; set; } = 0;

        /// <summary>
        /// The maximum size of the buffer used to receive UDP packets. Default is 8192 bytes.
        /// </summary>
        public int BufferSize { get; set; } = 8192;
    }
}
