using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    /// <summary>
    /// Helper methods for working with F1TelemetryFrame.
    /// Provides friendly API for deserializing packet data.
    /// </summary>
    public static class F1TelemetryFrameExtensions
    {
        /// <summary>
        /// Gets the Type of the packet based on format and ID
        /// </summary>
        public static Type? GetPacketType(this F1TelemetryFrame frame)
        {
            return F1PacketTypeMapper.GetPacketType(frame.PacketFormat, frame.PacketId);
        }

        /// <summary>
        /// Deserializes the packet to the specified type
        /// </summary>
        public static unsafe T GetPacket<T>(this F1TelemetryFrame frame) where T : struct
        {
            var packetType = typeof(T);
            var expectedSize = Marshal.SizeOf<T>();

            if (frame.DataLength < expectedSize)
            {
                throw new InvalidOperationException(
                    $"Packet data too small. Expected {expectedSize} bytes for {packetType.Name}, got {frame.DataLength}");
            }

            return Unsafe.Read<T>(frame.RawData);
        }

        /// <summary>
        /// Deserializes the packet to its actual type (determined by PacketFormat and PacketId)
        /// </summary>
        public static unsafe object? DeserializePacket(this F1TelemetryFrame frame)
        {
            var packetType = frame.GetPacketType();
            if (packetType == null)
                return null;

            var expectedSize = Marshal.SizeOf(packetType);
            if (frame.DataLength < expectedSize)
            {
                throw new InvalidOperationException($"Packet data too small. Expected {expectedSize} bytes for {packetType.Name}, got {frame.DataLength}");
            }

            byte* ptr = frame.RawData;
            var result = Marshal.PtrToStructure((IntPtr)ptr, packetType);
            return result;
        }

        /// <summary>
        /// Gets the raw packet data as a byte array
        /// </summary>
        public static unsafe byte[] GetRawData(this F1TelemetryFrame frame)
        {
            var data = new byte[frame.DataLength];
            byte* ptr = frame.RawData;
            Marshal.Copy((IntPtr)ptr, data, 0, frame.DataLength);
            return data;
        }
    }
}
