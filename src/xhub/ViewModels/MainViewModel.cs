using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using xhub.Models;

namespace xhub.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _statusText = "Ready.";

    public ObservableCollection<ProgramViewModel> Programs { get; } = new();

    public string StatusText
    {
        get => _statusText;
        set
        {
            if (_statusText == value) return;
            _statusText = value;
            OnPropertyChanged();
        }
    }

    public MainViewModel()
    {
        Programs.Add(new ProgramViewModel(new ProgramInfo
        {
            Name = "Program A",
            Version = "v1.0.0",
            Description = "A sample utility for everyday tasks.",
            Status = InstallStatus.ReadyToInstall
        }));

        Programs.Add(new ProgramViewModel(new ProgramInfo
        {
            Name = "Program B",
            Version = "v2.1.0",
            Description = "Another tool for productivity.",
            Status = InstallStatus.Installed
        }));

        Programs.Add(new ProgramViewModel(new ProgramInfo
        {
            Name = "Program C",
            Version = "v1.5.0",
            Description = "Third application with enhanced features.",
            Status = InstallStatus.UpdateAvailable
        }));

        Programs.Add(new ProgramViewModel(new ProgramInfo
        {
            Name = "Program D",
            Version = "v3.0.1",
            Description = "Developer toolkit and utilities.",
            Status = InstallStatus.ReadyToInstall
        }));

        StatusText = $"{Programs.Count} programs available.";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
