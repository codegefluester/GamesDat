namespace GameasDat.Core.Attributes;

/// <summary>
/// Identifies which game generated the telemetry data.
/// Used by session file headers to embed game metadata.
/// </summary>
[AttributeUsage(AttributeTargets.Struct)]
public class GameIdAttribute : Attribute
{
    /// <summary>
    /// Short identifier for the game (e.g., "ACC", "F1", "iRacing")
    /// </summary>
    public string GameId { get; }

    public GameIdAttribute(string gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId))
            throw new ArgumentException("Game ID cannot be null or empty", nameof(gameId));

        GameId = gameId;
    }
}
