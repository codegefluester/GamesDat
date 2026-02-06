namespace GamesDat.Demo.Wpf.Models;

public class DetectedFile
{
    public required string FileName { get; init; }
    public required string FullPath { get; init; }
    public required string SourceName { get; init; }
    public required DateTime DetectedAt { get; init; }
    public required DateTime FileCreatedAt { get; init; }
    public required DateTime FileModifiedAt { get; init; }
    public required long FileSizeBytes { get; init; }

    public string FileSizeFormatted => FormatFileSize(FileSizeBytes);

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
