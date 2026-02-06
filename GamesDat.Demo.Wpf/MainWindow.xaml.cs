using System.Windows;
using GamesDat.Demo.Wpf.ViewModels;

namespace GamesDat.Demo.Wpf;

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
