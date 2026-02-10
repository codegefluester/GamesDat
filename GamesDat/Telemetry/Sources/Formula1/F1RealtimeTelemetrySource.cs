using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    public class F1RealtimeTelemetrySource : UdpSourceBase<F1TelemetryFrame>
    {
        private const int MinRoutingHeaderSize = 7;

        public F1RealtimeTelemetrySource(UdpSourceOptions options) : base(options)
        {
        }

        protected override IEnumerable<F1TelemetryFrame> ProcessData(byte[] data)
        {
            if (data.Length < MinRoutingHeaderSize)
            {
                System.Diagnostics.Debug.WriteLine($"[F1] Packet too small: {data.Length} bytes (min {MinRoutingHeaderSize})");
                yield break;
            }

            var packetFormat = BitConverter.ToUInt16(data, 0);
            var packetId = data[6];

            var packetType = F1PacketTypeMapper.GetPacketType(packetFormat, packetId);
            if (packetType == null)
            {
                System.Diagnostics.Debug.WriteLine($"[F1] Unknown packet: Format={packetFormat}, PacketId={packetId}");
                yield break;
            }

            var expectedSize = Marshal.SizeOf(packetType);
            if (data.Length < expectedSize)
            {
                System.Diagnostics.Debug.WriteLine($"[F1] Packet size mismatch: PacketId={packetId}, Expected={expectedSize}, Actual={data.Length}");
                yield break;
            }

            System.Diagnostics.Debug.WriteLine($"[F1] Packet received: Format={packetFormat}, PacketId={packetId}, Size={data.Length}");

            yield return new F1TelemetryFrame(packetFormat, packetId, data);
        }

        private static object BytesToStruct(byte[] bytes, Type type)
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type)!;
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
