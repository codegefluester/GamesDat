using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamesDat.Core.Telemetry.Sources;
using GamesDate.Demo.Wpf.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace GamesDate.Demo.Wpf.ViewModels;

public partial class FileWatcherSourceViewModel : ViewModelBase, IDisposable
{
    private readonly Func<FileWatcherSourceBase>? _sourceFactory;
    private FileWatcherSourceBase? _source;
    private CancellationTokenSource? _cts;
    private readonly SynchronizationContext? _syncContext;

    [ObservableProperty]
    private string _sourceName = "";

    [ObservableProperty]
    private string _watchPath = "";

    [ObservableProperty]
    private string _patterns = "";

    [ObservableProperty]
    private bool _isWatching;

    [ObservableProperty]
    private bool _pathExists;

    [ObservableProperty]
    private string _statusMessage = "Stopped";

    [ObservableProperty]
    private int _fileCount;

    public ObservableCollection<DetectedFile> DetectedFiles { get; } = [];

    public FileWatcherSourceViewModel(
        string sourceName,
        string watchPath,
        string patterns,
        Func<FileWatcherSourceBase>? sourceFactory)
    {
        _sourceName = sourceName;
        _watchPath = watchPath;
        _patterns = patterns;
        _sourceFactory = sourceFactory;
        _syncContext = SynchronizationContext.Current;

        PathExists = Directory.Exists(watchPath);
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        if (IsWatching || _sourceFactory == null) return;

        try
        {
            _source = _sourceFactory();
            _cts = new CancellationTokenSource();
            IsWatching = true;
            StatusMessage = "Watching...";

            await foreach (var filePath in _source.ReadContinuousAsync(_cts.Token))
            {
                var fileInfo = new FileInfo(filePath);
                var detectedFile = new DetectedFile
                {
                    FileName = fileInfo.Name,
                    FullPath = filePath,
                    SourceName = SourceName,
                    DetectedAt = DateTime.Now,
                    FileCreatedAt = fileInfo.Exists ? fileInfo.CreationTime : DateTime.MinValue,
                    FileModifiedAt = fileInfo.Exists ? fileInfo.LastWriteTime : DateTime.MinValue,
                    FileSizeBytes = fileInfo.Exists ? fileInfo.Length : 0
                };

                if (_syncContext != null)
                {
                    _syncContext.Post(_ =>
                    {
                        DetectedFiles.Insert(0, detectedFile);
                        FileCount = DetectedFiles.Count;
                    }, null);
                }
                else
                {
                    DetectedFiles.Insert(0, detectedFile);
                    FileCount = DetectedFiles.Count;
                }
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Stopped";
        }
        catch (DirectoryNotFoundException ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            PathExists = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsWatching = false;
        }
    }

    private bool CanStart() => !IsWatching && PathExists && _sourceFactory != null;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        _cts?.Cancel();
        _source?.Dispose();
        _source = null;
        IsWatching = false;
        StatusMessage = "Stopped";
        ClearFiles();
    }

    private bool CanStop() => IsWatching;

    [RelayCommand]
    private void ClearFiles()
    {
        DetectedFiles.Clear();
        FileCount = 0;
    }

    partial void OnIsWatchingChanged(bool value)
    {
        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
    }

    partial void OnPathExistsChanged(bool value)
    {
        StartCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
