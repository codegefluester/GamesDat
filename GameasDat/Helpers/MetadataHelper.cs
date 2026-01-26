using System.Reflection;
using GameasDat.Core.Attributes;

namespace GameasDat.Core.Helpers;

/// <summary>
/// Helper methods for extracting metadata from data structures
/// </summary>
public static class MetadataHelper
{
    /// <summary>
    /// Extracts the game identifier from a struct's GameId attribute
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the GameId attribute is missing</exception>
    public static string GetGameId<T>() where T : struct
    {
        var attr = typeof(T).GetCustomAttribute<GameIdAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException(
                $"Type {typeof(T).Name} is missing the [GameId] attribute. " +
                $"Add [GameId(\"GAME\")] to the struct definition.");
        }
        return attr.GameId;
    }

    /// <summary>
    /// Extracts the version information from a struct's DataVersion attribute
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the DataVersion attribute is missing</exception>
    public static (int major, int minor, int patch) GetDataVersion<T>() where T : struct
    {
        var attr = typeof(T).GetCustomAttribute<DataVersionAttribute>();
        if (attr == null)
        {
            throw new InvalidOperationException(
                $"Type {typeof(T).Name} is missing the [DataVersion] attribute. " +
                $"Add [DataVersion(major, minor, patch)] to the struct definition.");
        }
        return (attr.Major, attr.Minor, attr.Patch);
    }
}
