using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12024
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WeatherForecastSample
    {
        public byte m_sessionType;              // 0 = unknown, see appendix
        public byte m_timeOffset;               // Time in minutes the forecast is for
        public byte m_weather;                  // Weather - 0 = clear, 1 = light cloud, 2 = overcast, 3 = light rain, 4 = heavy rain, 5 = storm
        public sbyte m_trackTemperature;         // Track temp. in degrees celsius
        public sbyte m_trackTemperatureChange;   // Track temp. change - 0 = up, 1 = down, 2 = no change
        public sbyte m_airTemperature;           // Air temp. in degrees celsius
        public sbyte m_airTemperatureChange;     // Air temp. change - 0 = up, 1 = down, 2 = no change
        public byte m_rainPercentage;           // Rain percentage (0-100)
    }
}
