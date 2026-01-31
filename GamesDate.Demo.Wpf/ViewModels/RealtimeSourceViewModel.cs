using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamesDat.Core.Telemetry;
using GamesDat.Core.Telemetry.Sources;
using GamesDate.Demo.Wpf.Models;
using System.Collections.ObjectModel;

namespace GamesDate.Demo.Wpf.ViewModels;

public partial class RealtimeSourceViewModel : ViewModelBase, IDisposable
{
    private readonly Func<IDisposable>? _sourceFactory;
    private IDisposable? _source;
    private CancellationTokenSource? _cts;
    private readonly SynchronizationContext? _syncContext;

    [ObservableProperty]
    private string _sourceName = "";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusMessage = "Stopped";

    [ObservableProperty]
    private int _dataPointCount;

    [ObservableProperty]
    private string _currentSpeed = "0";

    [ObservableProperty]
    private string _currentGear = "0";

    [ObservableProperty]
    private string _currentRPM = "0";

    public ObservableCollection<TelemetryDataPoint> DataPoints { get; } = [];

    public RealtimeSourceViewModel(
        string sourceName,
        Func<IDisposable>? sourceFactory)
    {
        _sourceName = sourceName;
        _sourceFactory = sourceFactory;
        _syncContext = SynchronizationContext.Current;
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        if (IsRunning || _sourceFactory == null) return;

        try
        {
            var sourceObj = _sourceFactory();
            _source = sourceObj;
            _cts = new CancellationTokenSource();
            IsRunning = true;
            StatusMessage = "Running...";

            // Check if source is a ITelemetrySource<TrackmaniaDataV3>
            if (sourceObj is ITelemetrySource<GamesDat.Core.Telemetry.Sources.Trackmania.TrackmaniaDataV3> trackmaniaSource)
            {
                await foreach (var data in trackmaniaSource.ReadContinuousAsync(_cts.Token))
                {
                    if (_syncContext != null)
                    {
                        _syncContext.Post(_ =>
                        {
                            UpdateTrackmaniaDisplay(data);
                        }, null);
                    }
                    else
                    {
                        UpdateTrackmaniaDisplay(data);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Stopped";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
    }

    private void UpdateTrackmaniaDisplay(GamesDat.Core.Telemetry.Sources.Trackmania.TrackmaniaDataV3 data)
    {
        CurrentSpeed = $"{data.Vehicle.SpeedMeter} km/h";
        CurrentGear = data.Vehicle.EngineCurGear.ToString();
        CurrentRPM = $"{data.Vehicle.EngineRpm:F0}";

        // Extended diagnostic info
        var gameStateText = data.Game.State.ToString();
        var raceStateText = data.Race.State.ToString();

        var diagnosticInfo = $"GameState: {gameStateText}, " +
                           $"RaceState: {raceStateText}, " +
                           $"RaceTime: {data.Race.Time}ms, " +
                           $"Pos: ({data.Object.Translation.X:F1}, {data.Object.Translation.Y:F1}, {data.Object.Translation.Z:F1}), " +
                           $"Vel: ({data.Object.Velocity.X:F1}, {data.Object.Velocity.Y:F1}, {data.Object.Velocity.Z:F1}), " +
                           $"Checkpoints: {data.Race.NbCheckpoints}, " +
                           $"Player: '{data.GetPlayerName()}', " +
                           $"Map: '{data.GetMapName()}', " +
                           $"Variant: '{data.GetGameplayVariant()}'";

        var dataPoint = new TelemetryDataPoint
        {
            Timestamp = DateTime.Now,
            SourceName = SourceName,
            DataType = "Telemetry",
            Value = $"Speed: {data.Vehicle.SpeedMeter}, Gear: {data.Vehicle.EngineCurGear}, RPM: {data.Vehicle.EngineRpm:F0} | {diagnosticInfo}"
        };

        DataPoints.Insert(0, dataPoint);
        if (DataPoints.Count > 100) // Keep only last 100 data points
        {
            DataPoints.RemoveAt(DataPoints.Count - 1);
        }
        DataPointCount = DataPoints.Count;
    }

    private bool CanStart() => !IsRunning && _sourceFactory != null;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        _cts?.Cancel();
        _source?.Dispose();
        _source = null;
        IsRunning = false;
        StatusMessage = "Stopped";
    }

    private bool CanStop() => IsRunning;

    [RelayCommand]
    private void ClearData()
    {
        DataPoints.Clear();
        DataPointCount = 0;
        CurrentSpeed = "0";
        CurrentGear = "0";
        CurrentRPM = "0";
    }

    partial void OnIsRunningChanged(bool value)
    {
        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        Stop();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
