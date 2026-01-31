using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamesDat.Core;
using GamesDat.Core.Telemetry;
using GamesDat.Core.Telemetry.Sources;
using GamesDate.Demo.Wpf.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace GamesDate.Demo.Wpf.ViewModels;

public partial class RealtimeSourceViewModel : ViewModelBase, IDisposable
{
    private readonly Func<GameSession>? _sessionFactory;
    private GameSession? _session;
    private CancellationTokenSource? _cts;
    private readonly SynchronizationContext? _syncContext;
    private StringWriter? _consoleCapture;

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
        Func<GameSession>? sessionFactory)
    {
        _sourceName = sourceName;
        _sessionFactory = sessionFactory;
        _syncContext = SynchronizationContext.Current;
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        if (IsRunning || _sessionFactory == null) return;

        try
        {
            // Capture console output to see any errors
            _consoleCapture = new StringWriter();
            var originalOut = Console.Out;
            Console.SetOut(new TeeWriter(originalOut, _consoleCapture));

            _session = _sessionFactory();
            _cts = new CancellationTokenSource();
            IsRunning = true;
            StatusMessage = "Starting...";

            await _session.StartAsync(_cts.Token);

            // Give it a moment to start
            await Task.Delay(1000);

            // Check if we received any error messages
            var consoleOutput = _consoleCapture.ToString();
            if (consoleOutput.Contains("ERROR") || consoleOutput.Contains("Error"))
            {
                StatusMessage = $"Error: {consoleOutput.Split('\n').FirstOrDefault(l => l.Contains("ERROR") || l.Contains("Error")) ?? "Unknown error"}";
                IsRunning = false;
            }
            else if (DataPointCount == 0)
            {
                StatusMessage = "Waiting for data... (Make sure you're in a race)";
            }
            else
            {
                StatusMessage = "Running...";
            }

            // Session is now running in the background
            // Monitor console output for errors
            _ = MonitorSessionAsync();
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Stopped";
            IsRunning = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Startup Error: {ex.Message}";
            IsRunning = false;
        }
    }

    private async Task MonitorSessionAsync()
    {
        try
        {
            while (IsRunning && _cts != null && !_cts.Token.IsCancellationRequested)
            {
                await Task.Delay(2000, _cts.Token);

                // Check console output for errors
                if (_consoleCapture != null)
                {
                    var output = _consoleCapture.ToString();
                    var lines = output.Split('\n');
                    var errorLine = lines.LastOrDefault(l => l.Contains("ERROR") || l.Contains("Error"));

                    if (errorLine != null && !StatusMessage.Contains("Error"))
                    {
                        StatusMessage = $"Error: {errorLine.Trim()}";
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
    }

    // Helper class to write to multiple TextWriters
    private class TeeWriter : TextWriter
    {
        private readonly TextWriter _writer1;
        private readonly TextWriter _writer2;

        public TeeWriter(TextWriter writer1, TextWriter writer2)
        {
            _writer1 = writer1;
            _writer2 = writer2;
        }

        public override Encoding Encoding => _writer1.Encoding;

        public override void Write(char value)
        {
            _writer1.Write(value);
            _writer2.Write(value);
        }

        public override void WriteLine(string? value)
        {
            _writer1.WriteLine(value);
            _writer2.WriteLine(value);
        }
    }

    public void UpdateTrackmaniaDisplay(GamesDat.Core.Telemetry.Sources.Trackmania.TrackmaniaDataV3 data)
    {
        CurrentSpeed = $"{data.Vehicle.SpeedMeter} km/h";
        CurrentGear = data.Vehicle.EngineCurGear.ToString();
        CurrentRPM = $"{data.Vehicle.EngineRpm:F0}";

        // Update status to show we're receiving data
        if (DataPointCount == 0)
        {
            StatusMessage = "Receiving data...";
        }
        else if (DataPointCount % 100 == 0)
        {
            StatusMessage = $"Running... ({DataPointCount} frames)";
        }

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

    private bool CanStart() => !IsRunning && _sessionFactory != null;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task Stop()
    {
        _cts?.Cancel();
        if (_session != null)
        {
            await _session.StopAsync();
            await _session.DisposeAsync();
            _session = null;
        }
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
        _cts?.Cancel();
        _session?.StopAsync().GetAwaiter().GetResult();
        _session?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _session = null;
        _cts?.Dispose();
        _consoleCapture?.Dispose();
        GC.SuppressFinalize(this);
    }
}
