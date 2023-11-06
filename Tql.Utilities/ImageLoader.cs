using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace Tql.Utilities;

public static class ImageFactory
{
    public static BitmapImage CreateBitmapImage(Stream stream)
    {
        var bitmapImage = new BitmapImage();

        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = stream;
        bitmapImage.EndInit();

        bitmapImage.Freeze();

        return bitmapImage;
    }

    public static DrawingImage CreateSvgImage(Stream stream)
    {
        var settings = new WpfDrawingSettings
        {
            IncludeRuntime = true,
            TextAsGeometry = false,
            OptimizePath = true
        };

        using var reader = new FileSvgReader(settings);

        var drawGroup = reader.Read(stream);
        if (drawGroup != null)
        {
            var drawing = new DrawingImage(drawGroup);

            drawing.Freeze();

            return drawing;
        }

        throw new InvalidOperationException("Could not convert SVG image");
    }

    public static ImageSource FromEmbeddedResource(Type relativeTo, string name)
    {
        var resourceName = $"{relativeTo.Namespace}.{name}";
        using var stream = relativeTo.Assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
            throw new ArgumentException($"Cannot find resource '{resourceName}'");

        if (name.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            return CreateSvgImage(stream);
        return CreateBitmapImage(stream);
    }
}
