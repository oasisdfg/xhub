namespace xhub.Models;

public enum InstallStatus
{
    ReadyToInstall,
    Installed,
    UpdateAvailable
}

public class ProgramInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public InstallStatus Status { get; set; }

    /// <summary>Tag prefix used to identify releases, e.g. "stretchedres" or "playerlookup". Empty for xtool.</summary>
    public string TagPrefix { get; set; } = string.Empty;

    /// <summary>GitHub repository in owner/repo format, e.g. "oasisdfg/xhub".</summary>
    public string GitHubRepo { get; set; } = string.Empty;

    /// <summary>Executable file name, e.g. "stretchedres.exe".</summary>
    public string ExeName { get; set; } = string.Empty;

    /// <summary>Installer asset name as published on GitHub Releases, e.g. "stretchedres_setup.exe".</summary>
    public string InstallerAssetName { get; set; } = string.Empty;

    /// <summary>Full path to the installed executable, used to detect whether the program is installed.</summary>
    public string InstallPath { get; set; } = string.Empty;
}
