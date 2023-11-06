using Tql.Abstractions;
using Tql.Plugins.GitHub.Categories;
using Tql.Plugins.GitHub.Data;

namespace Tql.Plugins.GitHub.Support;

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

    public static async Task<string> GetSearchPrefix(
        RootItemDto dto,
        ICache<GitHubData> cache,
        bool includeOrganizations = true
    )
    {
        if (dto.Scope == RootItemScope.Global)
            return string.Empty;

        var data = await cache.Get();
        var connectionData = data.Connections.SingleOrDefault(p => p.Id == dto.Id);
        if (connectionData == null)
            return string.Empty;

        var sb = StringBuilderCache.Acquire();

        sb.Append("user:").Append(connectionData.UserName).Append(' ');

        if (includeOrganizations)
        {
            foreach (var organization in connectionData.Organizations)
            {
                sb.Append("org:").Append(organization).Append(' ');
            }
        }

        return StringBuilderCache.GetStringAndRelease(sb);
    }
}
