using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GamesDat.Core.Telemetry.Sources;
using GamesDat.Core.Telemetry.Sources.Counter_Strike;
using GamesDat.Core.Telemetry.Sources.Rainbow_Six;
using GamesDat.Core.Telemetry.Sources.Rocket_League;
using GamesDat.Core.Telemetry.Sources.Overwatch2;
using GamesDat.Core.Telemetry.Sources.PUBG;
using GamesDat.Core.Telemetry.Sources.Fortnite;
using GamesDat.Core.Telemetry.Sources.Valorant;
using GamesDat.Core.Telemetry.Sources.DOTA2;
using GamesDate.Demo.Wpf.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;

namespace GamesDate.Demo.Wpf.ViewModels;

public partial class FileWatcherTabViewModel : ViewModelBase, IDisposable
{
    public ObservableCollection<FileWatcherSourceViewModel> Sources { get; } = [];

    public ObservableCollection<DetectedFile> AllDetectedFiles { get; } = [];

    [ObservableProperty]
    private FileWatcherSourceViewModel? _selectedSource;

    [ObservableProperty]
    private string _customPath = "";

    [ObservableProperty]
    private string _customPatterns = "*.*";

    [ObservableProperty]
    private string _customName = "Custom";

    [ObservableProperty]
    private bool _customIncludeSubdirectories;

    public FileWatcherTabViewModel()
    {
        InitializeBuiltInSources();
    }

    private void InitializeBuiltInSources()
    {
        // Add Rocket League source
        try
        {
            var rlPath = RocketLeagueReplayFileSource.GetDefaultReplayPath();
            var rlSource = new FileWatcherSourceViewModel(
                "Rocket League",
                rlPath,
                "*.replay",
                () => new RocketLeagueReplayFileSource());

            rlSource.DetectedFiles.CollectionChanged += OnSourceFilesChanged;
            Sources.Add(rlSource);
        }
        catch (DirectoryNotFoundException)
        {
            // Game not installed - add disabled source
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Documents", "My Games", "Rocket League", "TAGame", "Demos");

            var rlSource = new FileWatcherSourceViewModel(
                "Rocket League (Not Installed)",
                defaultPath,
                "*.replay",
                null);
            Sources.Add(rlSource);
        }

        // Add Rainbow Six source
        try
        {
            var r6Path = RainbowSixReplayFileSource.GetDefaultReplayPath();
            var r6Source = new FileWatcherSourceViewModel(
                "Rainbow Six Siege",
                r6Path,
                "*.rec",
                () => new RainbowSixReplayFileSource());

            r6Source.DetectedFiles.CollectionChanged += OnSourceFilesChanged;
            Sources.Add(r6Source);
        }
        catch (DirectoryNotFoundException)
        {
            // Game not installed - add disabled source
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Documents", "My Games", "Rainbow Six - Siege");

            var r6Source = new FileWatcherSourceViewModel(
                "Rainbow Six Siege (Not Installed)",
                defaultPath,
                "*.rec",
                null);
            Sources.Add(r6Source);
        }

        try
        {
            var csgoPath = CounterStrikeDemoFileSource.GetDefaultDemoPath();
            var csgoSource = new FileWatcherSourceViewModel(
                "Counter-Strike: Global Offensive",
                csgoPath,
                "*.dem",
                () => new CounterStrikeDemoFileSource());
            csgoSource.DetectedFiles.CollectionChanged += OnSourceFilesChanged;
            Sources.Add(csgoSource);
        }
        catch (DirectoryNotFoundException)
        {
            // Game not installed - add disabled source
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Steam", "steamapps", "common", "Counter-Strike Global Offensive", "csgo");
            var csgoSource = new FileWatcherSourceViewModel(
                "Counter-Strike: Global Offensive (Not Installed)",
                defaultPath,
                "*.dem",
                null);
            Sources.Add(csgoSource);
        }

        // Add DOTA 2 source
        try
        {
            var dota2Path = DOTA2ReplayFileSource.GetDefaultReplayPath();
            var dota2Source = new FileWatcherSourceViewModel(
                "DOTA 2",
                dota2Path,
                "*.dem",
                () => new DOTA2ReplayFileSource());
            dota2Source.DetectedFiles.CollectionChanged += OnSourceFilesChanged;
            Sources.Add(dota2Source);
        }
        catch (DirectoryNotFoundException)
        {
            // Game not installed - add disabled source
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Steam", "steamapps", "common", "dota 2 beta", "game", "dota", "replays");
            var dota2Source = new FileWatcherSourceViewModel(
                "DOTA 2 (Not Installed)",
                defaultPath,
                "*.dem",
                null);
            Sources.Add(dota2Source);
        }
        catch (InvalidOperationException)
        {
            // Steam not installed or library not found
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Steam", "steamapps", "common", "dota 2 beta", "game", "dota", "replays");
            var dota2Source = new FileWatcherSourceViewModel(
                "DOTA 2 (Not Installed)",
                defaultPath,
                "*.dem",
                null);
            Sources.Add(dota2Source);
        }

        // Add Overwatch 2 source
        try
        {
            var ow2Path = Overwatch2HighlightFileSource.GetDefaultReplayPath();
            var ow2Source = new FileWatcherSourceViewModel(
                "Overwatch 2",
                ow2Path,
                "*.mp4",
                () => new Overwatch2HighlightFileSource());
            ow2Source.DetectedFiles.CollectionChanged += OnSourceFilesChanged;
            Sources.Add(ow2Source);
        }
        catch (DirectoryNotFoundException)
        {
            // Game not installed - add disabled source
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Overwatch", "videos", "overwatch");
            var ow2Source = new FileWatcherSourceViewModel(
                "Overwatch 2 (Not Installed)",
                defaultPath,
                "*.mp4",
                null);
            Sources.Add(ow2Source);
        }

        // Add Valorant source
        try
        {
            var valorantPath = ValorantReplayFileSource.GetDefaultReplayPath();
            var valorantSource = new FileWatcherSourceViewModel(
                "Valorant",
                valorantPath,
                "*.dem",
                () => new ValorantReplayFileSource());
            valorantSource.DetectedFiles.CollectionChanged += OnSourceFilesChanged;
            Sources.Add(valorantSource);
        }
        catch (DirectoryNotFoundException)
        {
            // Game not installed - add disabled source
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VALORANT", "Saved", "Demos");
            var valorantSource = new FileWatcherSourceViewModel(
                "Valorant (Not Installed)",
                defaultPath,
                "*.dem",
                null);
            Sources.Add(valorantSource);
        }

        // Add PUBG source
        try
        {
            var pubgPath = PUBGReplayFileSource.GetDefaultReplayPath();
            var pubgSource = new FileWatcherSourceViewModel(
                "PUBG: Battlegrounds",
                pubgPath,
                "*.replayinfo",
                () => new PUBGReplayFileSource());
            pubgSource.DetectedFiles.CollectionChanged += OnSourceFilesChanged;
            Sources.Add(pubgSource);
        }
        catch (DirectoryNotFoundException)
        {
            // Game not installed - add disabled source
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TslGame", "Saved", "Demos");
            var pubgSource = new FileWatcherSourceViewModel(
                "PUBG: Battlegrounds (Not Installed)",
                defaultPath,
                "*.replayinfo",
                null);
            Sources.Add(pubgSource);
        }

        // Add Fortnite source
        try
        {
            var fortnitePath = FortniteReplayFileSource.GetDefaultReplayPath();
            var fortniteSource = new FileWatcherSourceViewModel(
                "Fortnite",
                fortnitePath,
                "*.replay",
                () => new FortniteReplayFileSource());
            fortniteSource.DetectedFiles.CollectionChanged += OnSourceFilesChanged;
            Sources.Add(fortniteSource);
        }
        catch (DirectoryNotFoundException)
        {
            // Game not installed - add disabled source
            var defaultPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FortniteGame", "Saved", "Demos");
            var fortniteSource = new FileWatcherSourceViewModel(
                "Fortnite (Not Installed)",
                defaultPath,
                "*.replay",
                null);
            Sources.Add(fortniteSource);
        }
    }

    private void OnSourceFilesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // Find the source that was cleared and remove its files from AllDetectedFiles
            if (sender is ObservableCollection<DetectedFile> sourceCollection)
            {
                var sourceVm = Sources.FirstOrDefault(s => s.DetectedFiles == sourceCollection);
                if (sourceVm != null)
                {
                    var filesToRemove = AllDetectedFiles
                        .Where(f => f.SourceName == sourceVm.SourceName)
                        .ToList();
                    foreach (var file in filesToRemove)
                    {
                        AllDetectedFiles.Remove(file);
                    }
                }
            }
        }
        else if (e.NewItems != null)
        {
            foreach (DetectedFile file in e.NewItems)
            {
                AllDetectedFiles.Insert(0, file);
            }
        }
    }

    [RelayCommand]
    private void AddCustomSource()
    {
        if (string.IsNullOrWhiteSpace(CustomPath)) return;

        var patterns = CustomPatterns
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToArray();

        var path = CustomPath;
        var name = string.IsNullOrWhiteSpace(CustomName) ? "Custom" : CustomName;
        var includeSubDirs = CustomIncludeSubdirectories;

        var customSource = new FileWatcherSourceViewModel(
            name,
            path,
            string.Join(", ", patterns),
            () => new FileWatcherSource(path, patterns, includeSubDirs));

        customSource.DetectedFiles.CollectionChanged += OnSourceFilesChanged;
        Sources.Add(customSource);

        // Reset form
        CustomPath = "";
        CustomPatterns = "*.*";
        CustomName = "Custom";
        CustomIncludeSubdirectories = false;
    }

    [RelayCommand]
    private void RemoveSource(FileWatcherSourceViewModel source)
    {
        source.DetectedFiles.CollectionChanged -= OnSourceFilesChanged;
        source.Dispose();
        Sources.Remove(source);
    }

    [RelayCommand]
    private void StartAll()
    {
        foreach (var source in Sources.Where(s => !s.IsWatching && s.PathExists))
        {
            source.StartCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void StopAll()
    {
        foreach (var source in Sources.Where(s => s.IsWatching))
        {
            source.StopCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void BrowseForPath()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select folder to watch"
        };

        if (dialog.ShowDialog() == true)
        {
            CustomPath = dialog.FolderName;
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
