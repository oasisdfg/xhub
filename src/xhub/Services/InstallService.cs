using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace xhub.Services;

/// <summary>
/// Handles downloading installer files and running Inno Setup installers silently.
/// Also provides helpers to detect whether a program is installed and to read its version.
/// </summary>
public class InstallService
{
    private static readonly HttpClient _httpClient = new();

    static InstallService()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("xhub/1.0");
        _httpClient.Timeout = TimeSpan.FromMinutes(10);
    }

    /// <summary>Returns true when the executable exists at the given path.</summary>
    public bool IsInstalled(string installPath) => File.Exists(installPath);

    /// <summary>
    /// Returns the FileVersion string from the executable's version info,
    /// or null if the file does not exist.
    /// </summary>
    public string? GetInstalledVersion(string installPath)
    {
        if (!File.Exists(installPath)) return null;
        var info = FileVersionInfo.GetVersionInfo(installPath);
        return info.FileVersion;
    }

    /// <summary>
    /// Downloads <paramref name="url"/> to <paramref name="destPath"/>,
    /// reporting progress as a percentage (0–100) via <paramref name="progress"/>.
    /// </summary>
    public async Task DownloadFileAsync(string url, string destPath, IProgress<double>? progress = null)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None);

        var buffer = new byte[81920];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalBytesRead += bytesRead;

            if (progress != null && totalBytes > 0)
                progress.Report((double)totalBytesRead / totalBytes * 100.0);
        }
    }

    /// <summary>
    /// Runs an Inno Setup installer silently and waits for it to finish.
    /// Returns the installer's exit code.
    /// </summary>
    public async Task<int> RunInstallerAsync(string installerPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = installerPath,
            Arguments = "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART",
            UseShellExecute = true
        };

        var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start installer process.");

        await process.WaitForExitAsync();
        return process.ExitCode;
    }
}
