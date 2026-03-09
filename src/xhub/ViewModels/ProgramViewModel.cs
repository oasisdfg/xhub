using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using xhub.Models;

namespace xhub.ViewModels;

public class ProgramViewModel : INotifyPropertyChanged
{
    private InstallStatus _status;
    private bool _isInstalling;
    private double _progress;

    public string Name { get; }
    public string Version { get; }
    public string Description { get; }

    public InstallStatus Status
    {
        get => _status;
        private set
        {
            if (_status == value) return;
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(ButtonText));
            OnPropertyChanged(nameof(IsActionEnabled));
        }
    }

    public string StatusText => _status switch
    {
        InstallStatus.ReadyToInstall => "Ready to install",
        InstallStatus.Installed      => "Up to date",
        InstallStatus.UpdateAvailable => "Update available",
        _                             => string.Empty
    };

    public string ButtonText => _status switch
    {
        InstallStatus.ReadyToInstall  => "Install",
        InstallStatus.Installed       => "Installed",
        InstallStatus.UpdateAvailable => "Update",
        _                              => string.Empty
    };

    public bool IsActionEnabled => !_isInstalling && _status != InstallStatus.Installed;

    public bool IsInstalling
    {
        get => _isInstalling;
        private set
        {
            if (_isInstalling == value) return;
            _isInstalling = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsActionEnabled));
            OnPropertyChanged(nameof(ProgressVisibility));
        }
    }

    public double Progress
    {
        get => _progress;
        private set
        {
            if (Math.Abs(_progress - value) < 0.01) return;
            _progress = value;
            OnPropertyChanged();
        }
    }

    public Visibility ProgressVisibility => _isInstalling ? Visibility.Visible : Visibility.Collapsed;

    public ICommand ActionCommand { get; }

    public ProgramViewModel(ProgramInfo info)
    {
        Name = info.Name;
        Version = info.Version;
        Description = info.Description;
        _status = info.Status;

        ActionCommand = new RelayCommand(
            async _ => await ExecuteActionAsync(),
            _ => IsActionEnabled);
    }

    private async Task ExecuteActionAsync()
    {
        IsInstalling = true;
        Progress = 0;

        for (int i = 0; i <= 100; i += 2)
        {
            Progress = i;
            await Task.Delay(30);
        }

        Status = InstallStatus.Installed;
        IsInstalling = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
