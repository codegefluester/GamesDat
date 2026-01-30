using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamesDat.Core.Telemetry.Sources.Trackmania;
using System.Collections.ObjectModel;

namespace GamesDate.Demo.Wpf.ViewModels;

public partial class RealtimeTabViewModel : ViewModelBase, IDisposable
{
    public ObservableCollection<RealtimeSourceViewModel> Sources { get; } = [];

    [ObservableProperty]
    private RealtimeSourceViewModel? _selectedSource;

    public RealtimeTabViewModel()
    {
        InitializeBuiltInSources();
    }

    private void InitializeBuiltInSources()
    {
        // Add Trackmania source - first realtime source
        var trackmaniaSource = new RealtimeSourceViewModel(
            "Trackmania",
            () => TrackmaniaMemoryMappedSource.CreateTelemetrySource());

        Sources.Add(trackmaniaSource);
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
