using System.Runtime.InteropServices;

namespace GamesDat.Core.Telemetry.Sources.Formula1
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LiveryColour
    {
        public byte red;
        public byte green;
        public byte blue;
    }
}
