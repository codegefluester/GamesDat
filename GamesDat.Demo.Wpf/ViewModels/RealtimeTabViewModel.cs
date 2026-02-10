using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamesDat.Core;
using GamesDat.Core.Telemetry.Sources.Trackmania;
using GamesDat.Core.Writer;
using System.Collections.ObjectModel;

namespace GamesDat.Demo.Wpf.ViewModels;

public partial class RealtimeTabViewModel : ViewModelBase, IDisposable
{
    public ObservableCollection<IRealtimeSource> Sources { get; } = [];

    [ObservableProperty]
    private IRealtimeSource? _selectedSource;

    public RealtimeTabViewModel()
    {
        InitializeBuiltInSources();
    }

    private void InitializeBuiltInSources()
    {
        // Add Trackmania source - first realtime source
        RealtimeSourceViewModel? trackmaniaSourceRef = null;

        var trackmaniaSource = new RealtimeSourceViewModel(
            "Trackmania",
            () =>
            {
                var outputPath = $"./sessions/trackmania_session_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.dat";

                var source = TrackmaniaMemoryMappedSource
                    .CreateTelemetrySource()
                    .OutputTo(outputPath)
                    .UseWriter(new BinarySessionWriter());

                var session = new GameSession(defaultOutputDirectory: "./sessions")
                    .AddSource(source)
                    .OnData<TrackmaniaDataV3>(data =>
                    {
                        // Real-time callback for UI updates
                        // Update happens on background thread, so marshal to UI thread
                        try
                        {
                            var dispatcher = System.Windows.Application.Current?.Dispatcher;
                            if (dispatcher != null)
                            {
                                dispatcher.BeginInvoke(() =>
                                {
                                    try
                                    {
                                        trackmaniaSourceRef?.UpdateTrackmaniaDisplay(data);
                                    }
                                    catch (Exception ex)
                                    {
                                        if (trackmaniaSourceRef != null)
                                            trackmaniaSourceRef.StatusMessage = $"UI Error: {ex.Message}";
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Callback error: {ex.Message}");
                        }
                    });

                return session;
            }
        );

        trackmaniaSourceRef = trackmaniaSource;
        Sources.Add(trackmaniaSource);

        // Add F1 source
        var f1Source = new F1RealtimeSourceViewModel();
        Sources.Add(f1Source);
    }

    [RelayCommand]
    private void StartAll()
    {
        foreach (var source in Sources.Where(s => !s.IsRunning))
        {
            source.StartCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void StopAll()
    {
        foreach (var source in Sources.Where(s => s.IsRunning))
        {
            source.StopCommand.Execute(null);
        }
    }

    public void Dispose()
    {
        foreach (var source in Sources)
        {
            source.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
