using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamesDat.Core;
using GamesDat.Core.Telemetry.Sources;
using GamesDat.Core.Telemetry.Sources.Formula1;
using GamesDat.Core.Writer;
using System.Windows.Input;

namespace GamesDat.Demo.Wpf.ViewModels;

public partial class F1RealtimeSourceViewModel : ViewModelBase, IRealtimeSource
{
    private GameSession? _gameSession;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private string _sourceName = "Formula 1";

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusMessage = "Stopped";

    [ObservableProperty]
    private int _totalPacketsReceived;

    [ObservableProperty]
    private string? _sessionOutputPath;

    [ObservableProperty]
    private int _port = 20777;

    // Explicit interface implementations for command covariance
    ICommand IRealtimeSource.StartCommand => StartCommand;
    ICommand IRealtimeSource.StopCommand => StopCommand;
    ICommand IRealtimeSource.ClearDataCommand => ClearDataCommand;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        if (IsRunning) return;

        try
        {
            StatusMessage = "Starting...";

            // Create F1 realtime telemetry source
            var options = new UdpSourceOptions
            {
                Port = Port
            };

            // Set output path for session recording
            SessionOutputPath = $"./sessions/f1_session_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.dat";

            F1RealtimeTelemetrySource f1Source;
            try
            {
                f1Source = new F1RealtimeTelemetrySource(options);
                f1Source.OutputTo(SessionOutputPath).UseWriter(new BinarySessionWriter());
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                StatusMessage = $"Failed to bind to port {Port}: {ex.Message}. Another application may be using this port.";
                IsRunning = false;
                return;
            }

            // Create game session
            _gameSession = new GameSession(defaultOutputDirectory: "./sessions")
                .AddSource(f1Source);

            // Simple callback to count packets
            _gameSession.OnData<F1TelemetryFrame>(frame =>
            {
                Interlocked.Increment(ref _totalPacketsReceived);

                // Update status message periodically
                var count = _totalPacketsReceived;
                if (count == 1)
                {
                    StatusMessage = $"Receiving data on UDP port {Port}";
                }
                else if (count % 100 == 0)
                {
                    StatusMessage = $"Running... ({count} packets)";
                }
            });

            // Start the session
            _cts = new CancellationTokenSource();
            IsRunning = true;

            // Start game session in background
            _ = Task.Run(async () =>
            {
                try
                {
                    await _gameSession.StartAsync(_cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Session Error: {ex.Message}";
                    IsRunning = false;
                }
            });

            // Give it a moment to start
            await Task.Delay(500);

            StatusMessage = $"Listening on UDP port {Port} (Waiting for F1 game data...)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Startup Error: {ex.Message}";
            IsRunning = false;
        }
    }

    private bool CanStart() => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task StopAsync()
    {
        _cts?.Cancel();

        if (_gameSession != null)
        {
            await _gameSession.StopAsync();
            await _gameSession.DisposeAsync();
            _gameSession = null;
        }

        IsRunning = false;
        StatusMessage = "Stopped";
    }

    private bool CanStop() => IsRunning;

    [RelayCommand]
    private void ClearData()
    {
        TotalPacketsReceived = 0;
    }

    partial void OnIsRunningChanged(bool value)
    {
        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _gameSession?.StopAsync().GetAwaiter().GetResult();
        _gameSession?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _gameSession = null;
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
