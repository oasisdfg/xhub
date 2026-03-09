using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using xhub.Models;
using xhub.Services;

namespace xhub.ViewModels;

public class ProgramViewModel : INotifyPropertyChanged
{
    private readonly ProgramInfo _info;
    private readonly InstallService _installService = new();

    private InstallStatus _status;
    private bool _isInstalling;
    private double _progress;
    private string _version;
    private string _latestDownloadUrl = string.Empty;

    public string Name { get; }
    public string Description { get; }

    /// <summary>Exposes the underlying ProgramInfo for assembly-internal use (e.g. MainViewModel).</summary>
    internal ProgramInfo Info => _info;

    public string Version
    {
        get => _version;
        private set
        {
            if (_version == value) return;
            _version = value;
            OnPropertyChanged();
        }
    }

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
        InstallStatus.ReadyToInstall  => "Ready to install",
        InstallStatus.Installed       => "Up to date",
        InstallStatus.UpdateAvailable => "Update available",
        _                              => string.Empty
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
        _info = info;
        Name = info.Name;
        _version = info.Version;
        _status = info.Status;
        Description = info.Description;

        ActionCommand = new RelayCommand(
            async _ => await ExecuteActionAsync(),
            _ => IsActionEnabled);
    }

    /// <summary>
    /// Called by MainViewModel after checking GitHub Releases and the local install state.
    /// Updates the displayed version, the latest download URL, and the install status.
    /// </summary>
    internal void UpdateFromCheck(InstallStatus status, string latestVersion, string downloadUrl)
    {
        _latestDownloadUrl = downloadUrl;
        if (!string.IsNullOrEmpty(latestVersion))
            Version = $"v{latestVersion}";
        Status = status;
    }

    private async Task ExecuteActionAsync()
    {
        if (string.IsNullOrEmpty(_latestDownloadUrl))
            return;

        IsInstalling = true;
        Progress = 0;

        var tempPath = Path.Combine(Path.GetTempPath(), _info.InstallerAssetName);
        try
        {
            var progressReporter = new Progress<double>(p => Progress = p);
            await _installService.DownloadFileAsync(_latestDownloadUrl, tempPath, progressReporter);

            Progress = 100;
            var exitCode = await _installService.RunInstallerAsync(tempPath);
            Status = exitCode == 0 ? InstallStatus.Installed : InstallStatus.ReadyToInstall;
        }
        finally
        {
            IsInstalling = false;
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
