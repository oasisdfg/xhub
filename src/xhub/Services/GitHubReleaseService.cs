using System.Net.Http;
using System.Text.Json;

namespace xhub.Services;

public class ReleaseInfo
{
    public string Version { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
}

/// <summary>
/// Queries the GitHub Releases API to find the latest release for a given repository and tag prefix.
/// No authentication is required for public repositories.
/// </summary>
public class GitHubReleaseService
{
    private static readonly HttpClient _httpClient = new();

    static GitHubReleaseService()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("xhub/1.0");
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Returns the latest release that matches the given tag prefix and contains the specified installer asset.
    /// </summary>
    /// <param name="repo">GitHub repository in "owner/repo" format.</param>
    /// <param name="tagPrefix">
    /// Tag prefix without the "-v" separator, e.g. "stretchedres" or "playerlookup".
    /// Pass an empty string for tools whose tags start directly with "v" (e.g. xtool).
    /// </param>
    /// <param name="installerAssetName">Name of the installer asset to look for, e.g. "stretchedres_setup.exe".</param>
    public async Task<ReleaseInfo?> GetLatestReleaseAsync(string repo, string tagPrefix, string installerAssetName)
    {
        var url = $"https://api.github.com/repos/{repo}/releases";
        var json = await _httpClient.GetStringAsync(url);

        using var doc = JsonDocument.Parse(json);
        var releases = doc.RootElement;

        ReleaseInfo? best = null;
        Version? bestVersion = null;

        foreach (var release in releases.EnumerateArray())
        {
            var tagName = release.GetProperty("tag_name").GetString() ?? string.Empty;

            // Determine the version string based on whether a tag prefix is used.
            string versionStr;
            if (!string.IsNullOrEmpty(tagPrefix))
            {
                var expectedPrefix = tagPrefix + "-v";
                if (!tagName.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
                    continue;
                versionStr = tagName[expectedPrefix.Length..];
            }
            else
            {
                if (!tagName.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                    continue;
                versionStr = tagName[1..];
            }

            if (!Version.TryParse(versionStr, out var ver))
                continue;

            if (bestVersion != null && ver <= bestVersion)
                continue;

            // Look for the installer asset inside this release.
            var downloadUrl = string.Empty;
            foreach (var asset in release.GetProperty("assets").EnumerateArray())
            {
                var assetName = asset.GetProperty("name").GetString() ?? string.Empty;
                if (assetName.Equals(installerAssetName, StringComparison.OrdinalIgnoreCase))
                {
                    downloadUrl = asset.GetProperty("browser_download_url").GetString() ?? string.Empty;
                    break;
                }
            }

            if (string.IsNullOrEmpty(downloadUrl))
                continue;

            bestVersion = ver;
            best = new ReleaseInfo
            {
                Version = versionStr,
                TagName = tagName,
                DownloadUrl = downloadUrl
            };
        }

        return best;
    }
}
