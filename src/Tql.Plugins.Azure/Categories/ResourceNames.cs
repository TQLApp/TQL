using System.Collections.Concurrent;
using System.IO.Compression;
using System.Xml.Linq;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using Tql.Utilities;

// ReSharper disable NotAccessedPositionalProperty.Local

namespace Tql.Plugins.Azure.Categories;

internal static class ResourceNames
{
    private static readonly Dictionary<string, AssetType> ResourceTypes = LoadResourceTypes();
    private static readonly Dictionary<int, AssetIcon> Icons = LoadIcons();
    private static readonly ConcurrentDictionary<AssetIcon, DrawingImage?> ImageCache = new();

    private static AssetCollection LoadAssetCollection()
    {
        using var stream = typeof(ResourceNames).Assembly.GetManifestResourceStream(
            $"{typeof(ResourceNames).Namespace}.Resources.json.gz"
        );
        using var gzStream = new GZipStream(stream!, CompressionMode.Decompress);

        return JsonSerializer.Deserialize<AssetCollection>(
            gzStream,
            new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            }
        )!;
    }

    private static Dictionary<string, AssetType> LoadResourceTypes()
    {
        return LoadAssetCollection()
            .Assets.ToDictionary(p => p.ResourceTypeName, p => p, StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<int, AssetIcon> LoadIcons()
    {
        return LoadAssetCollection().Icons.ToDictionary(p => p.Type, p => p);
    }

    public static ResourceName? GetResourceName(string resourceTypeName, string kind)
    {
        if (!ResourceTypes.TryGetValue(resourceTypeName, out var assetType))
            return null;

        if (!kind.IsEmpty())
        {
            var assetKind = assetType.Kinds?.FirstOrDefault(
                p =>
                    p.Kinds != null
                    && p.Kinds.Any(
                        p1 => string.Equals(p1, kind, StringComparison.OrdinalIgnoreCase)
                    )
            );

            if (assetKind is { SingularDisplayName: not null, PluralDisplayName: not null })
            {
                return new ResourceName(
                    assetKind.SingularDisplayName,
                    assetKind.PluralDisplayName,
                    GetIcon(assetKind.Icon ?? assetType.Icon)
                );
            }
        }

        if (assetType.SingularDisplayName != null && assetType.PluralDisplayName != null)
            return new ResourceName(
                assetType.SingularDisplayName,
                assetType.PluralDisplayName,
                GetIcon(assetType.Icon)
            );

        return null;
    }

    private static DrawingImage? GetIcon(AssetIcon? icon)
    {
        if (icon != null)
            return ImageCache.GetOrAdd(icon, CreateIcon);
        return null;
    }

    private static DrawingImage? CreateIcon(AssetIcon icon)
    {
        var settings = new WpfDrawingSettings
        {
            IncludeRuntime = true,
            TextAsGeometry = false,
            OptimizePath = true
        };

        if (icon.Data == null)
        {
            if (Icons.TryGetValue(icon.Type, out icon!))
                return GetIcon(icon);
            return null;
        }

        var doc = XDocument.Parse(icon.Data);
        // Set the default color to white instead of black.
        doc.Root!.SetAttributeValue("fill", "white");

        using var reader = new StringReader(doc.ToString());
        using var svgReader = new FileSvgReader(settings);

        var drawGroup = svgReader.Read(reader);
        if (drawGroup != null)
        {
            var image = new DrawingImage(drawGroup);

            image.Freeze();

            return image;
        }

        throw new InvalidOperationException("Could not convert SVG image");
    }

    private record AssetCollection(List<AssetType> Assets, List<AssetIcon> Icons);

    record AssetType(
        string Name,
        string? SingularDisplayName,
        string? PluralDisplayName,
        string ResourceTypeName,
        AssetIcon? Icon,
        List<AssetKind>? Kinds
    );

    record AssetKind(
        string Name,
        string? SingularDisplayName,
        string? PluralDisplayName,
        List<string>? Kinds,
        AssetIcon? Icon
    );

    record AssetIcon(int Type, string? Data);
}

internal record ResourceName(
    string SingularDisplayName,
    string PluralDisplayName,
    DrawingImage? Icon
);
