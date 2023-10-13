namespace Launcher.Plugins.GitHub.Support;

internal static class GitHubUtils
{
    public static string GetRepositoryName(string htmlUrl)
    {
        var uri = new Uri(htmlUrl);
        var path = uri.LocalPath.TrimStart('/');
        int pos = path.IndexOf('/');
        if (pos != -1)
        {
            pos = path.IndexOf('/', pos + 1);
            if (pos != -1)
                path = path.Substring(0, pos);
        }
        return path;
    }
}
