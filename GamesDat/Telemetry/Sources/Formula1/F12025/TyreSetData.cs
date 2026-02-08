using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TyreSetData
    {
        public byte m_actualTyreCompound;               // Actual tyre compound used
        public byte m_visualTyreCompound;               // Visual tyre compound used
        public byte m_wear;                             // Tyre wear (percentage)
        public byte m_available;                        // Whether this set is currently available
        public byte m_recommendedSession;               // Recommended session for tyre set, see appendix
        public byte m_lifeSpan;                         // Laps left in this tyre set
        public byte m_usableLife;                       // Max number of laps recommended for this compound
        public short m_lapDeltaTime;                    // Lap delta time in milliseconds compared to fitted set
        public byte m_fitted;                           // Whether the set is fitted or not
    }
}
