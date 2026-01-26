namespace GamesDat.Core.Attributes;

/// <summary>
/// Specifies the semantic version of a data structure.
/// Used for compatibility checking when reading session files.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public class DataVersionAttribute : Attribute
{
    /// <summary>
    /// Major version number (0-255)
    /// Increment for breaking changes to the struct layout
    /// </summary>
    public int Major { get; }

    /// <summary>
    /// Minor version number (0-255)
    /// Increment for backward-compatible additions
    /// </summary>
    public int Minor { get; }

    /// <summary>
    /// Patch version number (0-65535)
    /// Increment for bug fixes or clarifications
    /// </summary>
    public int Patch { get; }

    public DataVersionAttribute(int major, int minor, int patch)
    {
        if (major < 0 || major > 255)
            throw new ArgumentOutOfRangeException(nameof(major), "Major version must be 0-255");
        if (minor < 0 || minor > 255)
            throw new ArgumentOutOfRangeException(nameof(minor), "Minor version must be 0-255");
        if (patch < 0 || patch > 65535)
            throw new ArgumentOutOfRangeException(nameof(patch), "Patch version must be 0-65535");

        Major = major;
        Minor = minor;
        Patch = patch;
    }
}
