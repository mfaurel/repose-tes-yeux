using System.Reflection;
using System.Text.RegularExpressions;

namespace ReposeTesYeux.Timer;

public static class UpdateChecker
{
    // Update this constant to match the actual GitHub repository.
    private const string GitHubReleasesApi =
        "https://api.github.com/repos/YOUR_ORG/repose-tes-yeux/releases/latest";

    public static string CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1.0";

    /// <summary>
    /// Returns the latest version string if a newer release exists on GitHub, otherwise null.
    /// </summary>
    public static async Task<string?> CheckAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ReposeTesYeux/" + CurrentVersion);
            var json = await client.GetStringAsync(GitHubReleasesApi);
            var match = Regex.Match(json, "\"tag_name\"\\s*:\\s*\"v?([^\"]+)\"");
            if (!match.Success)
                return null;

            var latestStr = match.Groups[1].Value;
            if (Version.TryParse(latestStr, out var latest) &&
                Version.TryParse(CurrentVersion, out var current) &&
                latest > current)
                return latestStr;
        }
        catch { }
        return null;
    }
}
