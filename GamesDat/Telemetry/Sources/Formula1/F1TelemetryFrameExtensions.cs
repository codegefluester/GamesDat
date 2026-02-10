using System;
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

            return Marshal.PtrToStructure<T>((IntPtr)frame.RawData);
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

            var msg = $"[F1 Deserialize] Type={packetType.Name}, Expected={expectedSize}, Actual={frame.DataLength}, PacketId={frame.PacketId}";
            System.Diagnostics.Debug.WriteLine(msg);
            Console.WriteLine(msg);

            if (frame.DataLength < expectedSize)
            {
                var errorMsg = $"Packet data too small. Expected {expectedSize} bytes for {packetType.Name}, got {frame.DataLength}";
                Console.WriteLine($"[F1 ERROR] {errorMsg}");
                throw new InvalidOperationException(errorMsg);
            }

            try
            {
                var result = Marshal.PtrToStructure((IntPtr)frame.RawData, packetType);
                Console.WriteLine($"[F1 Deserialize] SUCCESS for PacketId={frame.PacketId}");
                return result;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Marshal.PtrToStructure failed: {ex.GetType().Name} - {ex.Message}";
                Console.WriteLine($"[F1 ERROR] {errorMsg}");
                System.Diagnostics.Debug.WriteLine($"[F1 ERROR] {errorMsg}");
                throw;
            }
        }

        /// <summary>
        /// Gets the raw packet data as a byte array
        /// </summary>
        public static unsafe byte[] GetRawData(this F1TelemetryFrame frame)
        {
            var data = new byte[frame.DataLength];
            Marshal.Copy((IntPtr)frame.RawData, data, 0, frame.DataLength);
            return data;
        }
    }
}
