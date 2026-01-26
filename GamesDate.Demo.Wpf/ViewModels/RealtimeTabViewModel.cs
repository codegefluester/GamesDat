using CommunityToolkit.Mvvm.ComponentModel;

namespace GamesDate.Demo.Wpf.ViewModels;

public partial class RealtimeTabViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    private string _placeholderText = "Realtime telemetry data will appear here (future implementation)";

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
