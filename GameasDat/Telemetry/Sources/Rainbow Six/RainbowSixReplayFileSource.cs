using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameasDat.Core.Telemetry.Sources.Rainbow_Six
{
    public static class RainbowSixReplayFileSource
    {
        /// <summary>
        /// Get the default Rainbow Six Siege replay folder path
        /// </summary>
        public static string GetDefaultReplayPath()
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            // R6 stores replays in Documents\My Games\Rainbow Six - Siege\[profile_id]\replays
            // We'll need to scan for profile folders
            var baseGamePath = Path.Combine(userProfile, "Documents", "My Games", "Rainbow Six - Siege");

            if (!Directory.Exists(baseGamePath))
                throw new DirectoryNotFoundException($"Rainbow Six Siege game folder not found: {baseGamePath}");

            // Find first profile folder with replays subfolder
            var profileDirs = Directory.GetDirectories(baseGamePath);
            foreach (var profileDir in profileDirs)
            {
                var replaysPath = Path.Combine(profileDir, "replays");
                if (Directory.Exists(replaysPath))
                    return replaysPath;
            }

            throw new DirectoryNotFoundException("Rainbow Six Siege replay folder not found in any profile");
        }

        /// <summary>
        /// Create a source that monitors for new Rainbow Six Siege replay files
        /// </summary>
        /// <param name="customPath">Optional custom replay folder path. If null, uses default location.</param>
        public static FileWatcherSource CreateReplaySource(string? customPath = null)
        {
            var path = customPath ?? GetDefaultReplayPath();

            return new FileWatcherSource(
                path: path,
                pattern: "*.rec", // R6 Siege replay files
                includeSubdirectories: false,
                debounceDelay: TimeSpan.FromSeconds(3)
            );
        }
    }
}
