using System.Windows;
using GamesDate.Demo.Wpf.ViewModels;

namespace GamesDate.Demo.Wpf;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainWindowViewModel();
        DataContext = _viewModel;

        Closed += (s, e) => _viewModel.Dispose();
    }
}
