using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    internal class F1RealtimeTelemetrySource : UdpSourceBase<F1TelemetryFrame>
    {
        private const int MinRoutingHeaderSize = 7;

        public F1RealtimeTelemetrySource(UdpSourceOptions options) : base(options)
        {
        }

        protected override IEnumerable<F1TelemetryFrame> ProcessData(byte[] data)
        {
            if (data.Length < MinRoutingHeaderSize)
                yield break;

            var packetFormat = BitConverter.ToUInt16(data, 0);
            var packetId = data[6];

            var packetType = F1PacketTypeMapper.GetPacketType(packetFormat, packetId);
            if (packetType == null)
                yield break;

            var expectedSize = Marshal.SizeOf(packetType);
            if (data.Length < expectedSize)
                yield break;

            var packet = BytesToStruct(data, packetType);

            yield return new F1TelemetryFrame
            {
                Packet = packet,
                PacketFormat = packetFormat,
                PacketId = packetId,
                PacketType = packetType
            };
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
