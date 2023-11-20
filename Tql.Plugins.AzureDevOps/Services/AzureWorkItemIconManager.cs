using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Services;

internal class AzureWorkItemIconManager
{
    private readonly ICache<AzureData> _cache;
    private readonly Dictionary<
        (string CollectionUrl, string Project, string WorkItemType),
        ImageSource
    > _images = new();
    private readonly object _syncRoot = new();

    public AzureWorkItemIconManager(ICache<AzureData> cache)
    {
        _cache = cache;
    }

    public ImageSource? GetWorkItemIconImage(
        string collectionUrl,
        string project,
        string workItemType
    )
    {
        lock (_syncRoot)
        {
            var key = (
                collectionUrl.ToLowerInvariant(),
                project.ToLowerInvariant(),
                workItemType.ToLowerInvariant()
            );

            if (_images.TryGetValue(key, out var value))
                return value;

            var task = _cache.Get();
            if (!task.IsCompleted)
                return null;

            var workItemIcon = task.Result
                .GetConnection(collectionUrl)
                .Projects
                .SingleOrDefault(
                    p => string.Equals(p.Name, project, StringComparison.OrdinalIgnoreCase)
                )
                ?.WorkItemTypes
                .SingleOrDefault(
                    p => string.Equals(p.Name, workItemType, StringComparison.OrdinalIgnoreCase)
                )
                ?.Icon;

            if (workItemIcon == null)
                return null;

            using var stream = new MemoryStream(workItemIcon.Data);

            ImageSource imageSource = string.Equals(workItemIcon.MediaType, "image/svg+xml")
                ? ImageFactory.CreateSvgImage(stream)
                : ImageFactory.CreateBitmapImage(stream);

            _images[key] = imageSource;

            return imageSource;
        }
    }
}
