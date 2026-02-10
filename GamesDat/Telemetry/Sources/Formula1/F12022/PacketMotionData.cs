using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12022
{
    /// <summary>
    /// Motion data packet for F1 2022 telemetry.
    /// Frequency: Rate as specified in menus
    /// Size: 1464 bytes
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PacketMotionData
    {
        public PacketHeader m_header;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public CarMotionData[] m_carMotionData;    // Data for all cars on track

        // Extra player car ONLY data
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] m_suspensionPosition;       // Note: All wheel arrays have the following order: RL, RR, FL, FR
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] m_suspensionVelocity;       // RL, RR, FL, FR
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] m_suspensionAcceleration;   // RL, RR, FL, FR
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] m_wheelSpeed;               // Speed of each wheel
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] m_wheelSlip;                // Slip ratio for each wheel
        public float m_localVelocityX;             // Velocity in local space
        public float m_localVelocityY;             // Velocity in local space
        public float m_localVelocityZ;             // Velocity in local space
        public float m_angularVelocityX;           // Angular velocity x-component
        public float m_angularVelocityY;           // Angular velocity y-component
        public float m_angularVelocityZ;           // Angular velocity z-component
        public float m_angularAccelerationX;       // Angular acceleration x-component
        public float m_angularAccelerationY;       // Angular acceleration y-component
        public float m_angularAccelerationZ;       // Angular acceleration z-component
        public float m_frontWheelsAngle;           // Current front wheels angle in radians
    }
}
