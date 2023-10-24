using NeoSmart.AsyncLock;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Services;

internal class IconCacheManager
{
    private readonly JiraApi _api;
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<string, ImageSource> _cache = new();

    public IconCacheManager(JiraApi api)
    {
        _api = api;
    }

    public ImageSource? GetIcon(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        using (_lock.Lock())
        {
            _cache.TryGetValue(url!, out var result);

            return result;
        }
    }

    public async Task<ImageSource?> DownloadIcon(Connection connection, string? url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        using (await _lock.LockAsync())
        {
            if (!_cache.TryGetValue(url!, out var image))
            {
                image = await CreateImage(connection, url!);

                _cache.Add(url!, image);
            }

            return image;
        }
    }

    private Task<ImageSource> CreateImage(Connection connection, string url)
    {
        return _api.GetClient(connection)
            .DownloadFile<ImageSource>(
                url,
                async p =>
                {
                    using var stream = await p.ReadAsStreamAsync();

                    if (p.Headers.ContentType.MediaType == "image/svg+xml")
                        return ImageFactory.CreateSvgImage(stream);

                    return ImageFactory.CreateBitmapImage(stream);
                }
            );
    }
}
