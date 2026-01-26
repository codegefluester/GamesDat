using CommunityToolkit.Mvvm.ComponentModel;

namespace GamesDate.Demo.Wpf.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty]
    private FileWatcherTabViewModel _fileWatcherTab;

    [ObservableProperty]
    private RealtimeTabViewModel _realtimeTab;

    [ObservableProperty]
    private int _selectedTabIndex;

    public MainWindowViewModel()
    {
        _fileWatcherTab = new FileWatcherTabViewModel();
        _realtimeTab = new RealtimeTabViewModel();
    }

    public void Dispose()
    {
        FileWatcherTab.Dispose();
        RealtimeTab.Dispose();
        GC.SuppressFinalize(this);
    }
}
