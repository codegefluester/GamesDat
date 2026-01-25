using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameasDat.Core.Telemetry.Sources.AssettoCorsa
{
    /// <summary>
    /// Pre-configured sources for ACC telemetry
    /// </summary>
    public static class ACCSources
    {
        public static MemoryMappedFileSource<ACCPhysics> CreatePhysicsSource()
        {
            return new("Local\\acpmf_physics", TimeSpan.FromMilliseconds(10)); // 100Hz
        }

        public static MemoryMappedFileSource<ACCGraphics> CreateGraphicsSource()
        {
            return new("Local\\acpmf_graphics", TimeSpan.FromMilliseconds(100)); // 10Hz
        }

        public static MemoryMappedFileSource<ACCStatic> CreateStaticSource()
        {
            return new("Local\\acpmf_static", TimeSpan.FromSeconds(5)); // Rarely changes
        }
    }
}
