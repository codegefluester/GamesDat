namespace GamesDate.Demo.Wpf.Models;

public class TelemetryDataPoint
{
    public required DateTime Timestamp { get; init; }
    public required string SourceName { get; init; }
    public required string DataType { get; init; }
    public required string Value { get; init; }
}
