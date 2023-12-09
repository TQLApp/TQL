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

        return await GetSearchPrefix(dto.Id, cache, includeOrganizations);
    }

    public static async Task<string> GetSearchPrefix(
        Guid connectionId,
        ICache<GitHubData> cache,
        bool includeOrganizations = true
    )
    {
        var data = await cache.Get();
        var connectionData = data.Connections.SingleOrDefault(p => p.Id == connectionId);
        if (connectionData == null)
            return string.Empty;

        var userName = connectionData.UserName;

        var organizations = default(ImmutableArray<string>?);
        if (includeOrganizations)
            organizations = connectionData.Organizations;

        return GetSearchPrefix(userName, organizations);
    }

    public static string GetSearchPrefix(string userName, ImmutableArray<string>? organizations)
    {
        var sb = StringBuilderCache.Acquire();

        sb.Append("user:").Append(userName).Append(' ');

        if (organizations.HasValue)
        {
            foreach (var organization in organizations.Value)
            {
                sb.Append("org:").Append(organization).Append(' ');
            }
        }

        return StringBuilderCache.GetStringAndRelease(sb);
    }
}
