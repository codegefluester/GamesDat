using System;
using System.Runtime.InteropServices;
using GamesDat.Core.Attributes;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    /// <summary>
    /// Unmanaged struct containing raw F1 telemetry packet data.
    /// Use F1TelemetryFrameExtensions for easy deserialization.
    /// </summary>
    [GameId("F1")]
    [DataVersion(1, 0, 0)]
    public unsafe struct F1TelemetryFrame
    {
        public ushort PacketFormat;
        public byte PacketId;
        public fixed byte RawData[2048]; // Max F1 packet size
        public int DataLength;

        public F1TelemetryFrame(ushort packetFormat, byte packetId, byte[] data)
        {
            PacketFormat = packetFormat;
            PacketId = packetId;
            DataLength = Math.Min(data.Length, 2048);

            fixed (byte* ptr = RawData)
            {
                Marshal.Copy(data, 0, (IntPtr)ptr, DataLength);
            }
        }
    }
}
