using Tql.Abstractions;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;
using Path = System.IO.Path;

namespace Tql.Plugins.Jira.Services;

internal class IconCacheManager
{
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<string, ImageSource?> _diskCache = new();
    private readonly Dictionary<string, ImageSource?> _cache = new();
    private readonly string _iconCache;

    public IconCacheManager(IStore store)
    {
        _iconCache = Path.Combine(store.GetCacheFolder(JiraPlugin.Id), "Icons");

        Directory.CreateDirectory(_iconCache);
    }

    public ImageSource? GetIcon(string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        using (_lock.Lock())
        {
            if (_cache.TryGetValue(url, out var result))
                return result;

            if (!_diskCache.TryGetValue(url, out result))
            {
                result = LoadIconFromDisk(url);
                _diskCache.Add(url, result);
            }

            return result;
        }
    }

    public async Task<ImageSource?> LoadIcon(JiraClient client, string url)
    {
        if (string.IsNullOrEmpty(url))
            return null;

        using (await _lock.LockAsync())
        {
            if (!_cache.TryGetValue(url, out var image))
            {
                image = await DownloadImage(client, url);

                _cache.Add(url, image);
                _diskCache.Remove(url);
            }

            return image;
        }
    }

    private Task<ImageSource> DownloadImage(JiraClient client, string url)
    {
        return client.DownloadFile<ImageSource>(
            url,
            async p =>
            {
                using var source = await p.ReadAsStreamAsync();
                using var target = new MemoryStream();

                await source.CopyToAsync(target);

                var dto = new CacheDto(p.Headers.ContentType.MediaType, target.ToArray());

                SaveIconToDisk(url, dto);

                return CreateImage(dto);
            }
        );
    }

    private ImageSource? LoadIconFromDisk(string url)
    {
        var fileName = CreateIconCacheFileName(url);
        if (!File.Exists(fileName))
            return null;

        using var stream = File.OpenRead(fileName);

        var dto = JsonSerializer.Deserialize<CacheDto>(stream)!;

        return CreateImage(dto);
    }

    private void SaveIconToDisk(string url, CacheDto dto)
    {
        var fileName = CreateIconCacheFileName(url);
        using var stream = File.Create(fileName);

        JsonSerializer.Serialize(stream, dto);
    }

    private string CreateIconCacheFileName(string url)
    {
        var hash = Encryption.Hash(url);
        return Path.Combine(_iconCache, hash);
    }

    private ImageSource CreateImage(CacheDto dto)
    {
        using var stream = new MemoryStream(dto.Data);

        if (dto.MediaType == "image/svg+xml")
            return ImageFactory.CreateSvgImage(stream);

        return ImageFactory.CreateBitmapImage(stream);
    }

    private record CacheDto(string MediaType, byte[] Data);
}
