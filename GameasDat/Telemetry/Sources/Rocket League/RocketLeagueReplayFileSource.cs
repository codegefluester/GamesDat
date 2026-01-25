using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameasDat.Core.Telemetry.Sources.Rocket_League
{
    public static class RocketLeagueReplayFileSource
    {
        /// <summary>
        /// Get the default Rocket League replay folder path
        /// </summary>
        public static string GetDefaultReplayPath()
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.DoNotVerify);
            return Path.Combine(userProfile, "Documents", "My Games", "Rocket League", "TAGame", "Demos");
        }

        /// <summary>
        /// Create a source that monitors for new Rocket League replay files
        /// </summary>
        /// <param name="customPath">Optional custom replay folder path. If null, uses default location.</param>
        public static FileWatcherSource CreateReplaySource(string? customPath = null)
        {
            var path = customPath ?? GetDefaultReplayPath();

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(
                    $"Rocket League replay folder not found: {path}\n" +
                    "Make sure Rocket League is installed and you've played at least one game.");

            return new FileWatcherSource(
                path: path,
                pattern: "*.replay",
                includeSubdirectories: false,
                debounceDelay: TimeSpan.FromSeconds(2) // RL writes replays in chunks
            );
        }
    }
}
