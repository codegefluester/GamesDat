using System.Windows.Input;

namespace GamesDat.Demo.Wpf.ViewModels;

public interface IRealtimeSource : IDisposable
{
    string SourceName { get; }
    bool IsRunning { get; }
    string StatusMessage { get; }
    ICommand StartCommand { get; }
    ICommand StopCommand { get; }
    ICommand ClearDataCommand { get; }
}
