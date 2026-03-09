using System.Collections.ObjectModel;
using System.IO;
using xhub.Models;
using xhub.Services;

namespace xhub.ViewModels;

public class MainViewModel
{
    private static readonly GitHubReleaseService _releaseService = new();
    private static readonly InstallService _installService = new();

    public ObservableCollection<ProgramViewModel> Programs { get; } = new();

    public MainViewModel()
    {
        var pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

        Programs.Add(new ProgramViewModel(new ProgramInfo
        {
            Name = "xtool",
            Version = "v4.3.3",
            Description = "Utility toolkit",
            Status = InstallStatus.ReadyToInstall,
            TagPrefix = "",
            GitHubRepo = "oasisdfg/xtool",
            ExeName = "xtool.exe",
            InstallerAssetName = "xtool_setup.exe",
            InstallPath = Path.Combine(pf, "xhub", "xtool", "xtool.exe")
        }));

        Programs.Add(new ProgramViewModel(new ProgramInfo
        {
            Name = "stretchedres",
            Version = "v1.0.0",
            Description = "Custom stretched resolution tool",
            Status = InstallStatus.ReadyToInstall,
            TagPrefix = "stretchedres",
            GitHubRepo = "oasisdfg/xhub",
            ExeName = "stretchedres.exe",
            InstallerAssetName = "stretchedres_setup.exe",
            InstallPath = Path.Combine(pf, "xhub", "stretchedres", "stretchedres.exe")
        }));

        Programs.Add(new ProgramViewModel(new ProgramInfo
        {
            Name = "PlayerLookup",
            Version = "v1.0.0",
            Description = "Find Users Past Aliases",
            Status = InstallStatus.ReadyToInstall,
            TagPrefix = "playerlookup",
            GitHubRepo = "oasisdfg/xhub",
            ExeName = "PlayerLookup.exe",
            InstallerAssetName = "playerlookup_setup.exe",
            InstallPath = Path.Combine(pf, "xhub", "PlayerLookup", "PlayerLookup.exe")
        }));

        // Kick off async version checks without blocking the constructor.
        _ = CheckAllProgramsAsync();
    }

    private async Task CheckAllProgramsAsync()
    {
        var tasks = Programs.Select(CheckProgramAsync);
        await Task.WhenAll(tasks);
    }

    private static async Task CheckProgramAsync(ProgramViewModel vm)
    {
        try
        {
            var info = vm.Info;

            var release = await _releaseService.GetLatestReleaseAsync(
                info.GitHubRepo, info.TagPrefix, info.InstallerAssetName);

            if (release == null)
                return;

            var isInstalled = _installService.IsInstalled(info.InstallPath);
            InstallStatus status;

            if (isInstalled)
            {
                var installedVersion = _installService.GetInstalledVersion(info.InstallPath);
                var latestParsed = TryParseVersion(release.Version);
                var installedParsed = TryParseVersion(installedVersion);

                status = (latestParsed != null && installedParsed != null && latestParsed > installedParsed)
                    ? InstallStatus.UpdateAvailable
                    : InstallStatus.Installed;
            }
            else
            {
                status = InstallStatus.ReadyToInstall;
            }

            vm.UpdateFromCheck(status, release.Version, release.DownloadUrl);
        }
        catch (Exception ex)
        {
            // Log to debug output to help diagnose API/network issues without crashing the UI.
            System.Diagnostics.Debug.WriteLine($"[xhub] Version check failed for {vm.Name}: {ex.Message}");
        }
    }

    private static Version? TryParseVersion(string? versionStr)
    {
        if (string.IsNullOrEmpty(versionStr)) return null;
        var s = versionStr.TrimStart('v');
        return Version.TryParse(s, out var v) ? v : null;
    }
}
