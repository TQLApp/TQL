using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Services;

internal class IconCacheManager : IconCacheManager<IconKey>
{
    private readonly ConfigurationManager _configurationManager;

    public IconCacheManager(
        IStore store,
        ILogger<IconCacheManager> logger,
        ConfigurationManager configurationManager
    )
        : base(
            store,
            logger,
            new IconCacheManagerConfiguration(ConfluencePlugin.Id, TimeSpan.FromDays(3))
        )
    {
        _configurationManager = configurationManager;
    }

    protected override Task<IconData> LoadIcon(IconKey key)
    {
        var client = _configurationManager.GetClient(key.ConnectionUrl);

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
