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
}
