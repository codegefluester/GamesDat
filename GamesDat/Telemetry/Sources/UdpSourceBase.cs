using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace GamesDat.Core.Telemetry.Sources
{
    public abstract class UdpSourceBase<T> : TelemetrySourceBase<T>
    {

        protected UdpClient _listener;
        protected IPEndPoint _endpoint;
        protected bool _isListening;

        protected int Port { get; set; }
        protected int BufferSize { get; set; }

        public UdpSourceBase(UdpSourceOptions options) : base()
        {
            Port = options.Port;
            BufferSize = options.BufferSize;

            _endpoint = new IPEndPoint(IPAddress.Any, Port);
            _listener = new UdpClient(_endpoint);
        }

        public override async IAsyncEnumerable<T> ReadContinuousAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            _isListening = true;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var result = await _listener.ReceiveAsync(ct);
                    var data = result.Buffer;
                    // Process the received data and yield telemetry objects
                    foreach (var item in ProcessData(data))
                    {
                        yield return item;
                    }
                }
            }
            finally
            {
                _isListening = false;
                _listener.Dispose();
            }
        }

        abstract protected IEnumerable<T> ProcessData(byte[] data);
    }
}
