using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    public struct F1TelemetryFrame
    {
        public object Packet { get; init; }
        public ushort PacketFormat { get; init; }
        public byte PacketId { get; init; }
        public Type PacketType { get; init; }

        public T GetPacket<T>() where T : struct
        {
            if (Packet is T typed)
                return typed;
            throw new InvalidCastException($"Packet is {Packet?.GetType().Name ?? "null"}, not {typeof(T).Name}");
        }
    }
}
