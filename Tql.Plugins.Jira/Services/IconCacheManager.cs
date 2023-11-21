using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Services;

internal class IconCacheManager(
    IStore store,
    ILogger<IconCacheManager> logger,
    ConfigurationManager configurationManager
)
    : IconCacheManager<IconKey>(
        store,
        logger,
        new IconCacheManagerConfiguration(JiraPlugin.Id, TimeSpan.FromDays(3))
    )
{
    protected override Task<IconData> LoadIcon(IconKey key)
    {
        var client = configurationManager.GetClient(key.ConnectionUrl);

        return client.DownloadFile(
            key.Url,
            async p =>
            {
                using var source = await p.ReadAsStreamAsync();
                using var target = new MemoryStream();

                await source.CopyToAsync(target);

                return new IconData(p.Headers.ContentType.MediaType, target.ToArray());
            }
        );
    }
}

internal record IconKey(string ConnectionUrl, string Url);
