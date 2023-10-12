using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Utilities;
using System.IO;

namespace Launcher.Plugins.AzureDevOps.Services;

internal class AzureWorkItemIconManager
{
    private readonly ICache<AzureData> _cache;
    private readonly Dictionary<
        (string CollectionUrl, string Project, string WorkItemType),
        (AzureWorkItemIcon Icon, ImageSource ImageSource)
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
        var task = _cache.Get();
        if (!task.IsCompleted)
            return null;

        var key = (
            collectionUrl.ToLowerInvariant(),
            project.ToLowerInvariant(),
            workItemType.ToLowerInvariant()
        );

        var workItemIcon = task.Result
            .GetConnection(collectionUrl)
            .Projects.SingleOrDefault(
                p => string.Equals(p.Name, project, StringComparison.OrdinalIgnoreCase)
            )
            ?.WorkItemTypes.SingleOrDefault(
                p => string.Equals(p.Name, workItemType, StringComparison.OrdinalIgnoreCase)
            )
            ?.Icon;

        ImageSource imageSource;

        lock (_syncRoot)
        {
            if (_images.TryGetValue(key, out var value))
            {
                if (workItemIcon != null && value.Icon == workItemIcon)
                    return value.ImageSource;
            }

            if (workItemIcon == null)
                return null;

            using var stream = new MemoryStream(workItemIcon.Data);

            imageSource = string.Equals(workItemIcon.MediaType, "image/svg+xml")
                ? ImageFactory.CreateSvgImage(stream)
                : ImageFactory.CreateBitmapImage(stream);

            _images[key] = (workItemIcon, imageSource);
        }

        return imageSource;
    }
}
