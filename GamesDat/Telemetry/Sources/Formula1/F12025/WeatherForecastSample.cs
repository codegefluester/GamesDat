using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1.F12025
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WeatherForecastSample
    {
        // 0 = unknown, see appendix
        public byte m_sessionType;              // uint8

        // Time in minutes the forecast is for
        public byte m_timeOffset;               // uint8

        // Weather - 0 = clear, 1 = light cloud, 2 = overcast, 3 = light rain, 4 = heavy rain, 5 = storm
        public byte m_weather;                  // uint8

        // Track temp. in degrees celsius
        public sbyte m_trackTemperature;        // int8

        // Track temp. change - 0 = up, 1 = down, 2 = no change
        public sbyte m_trackTemperatureChange;  // int8

        // Air temp. in degrees celsius
        public sbyte m_airTemperature;          // int8

        // Air temp. change - 0 = up, 1 = down, 2 = no change
        public sbyte m_airTemperatureChange;    // int8

        // Rain percentage (0-100)
        public byte m_rainPercentage;           // uint8
    }
}
