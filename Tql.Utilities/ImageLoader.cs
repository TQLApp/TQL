using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using Brush = System.Windows.Media.Brush;

namespace Tql.Utilities;

/// <summary>
/// Utility methods for loading images.
/// </summary>
/// <remarks>
/// These methods are optimized for loading match icons. They ensure
/// that the images are fully initialized and frozen to optimize
/// for performance and memory usage.
/// </remarks>
public static class ImageFactory
{
    /// <summary>
    /// Loads a bitmap image like a PNG or JPEG image.
    /// </summary>
    /// <param name="stream">Stream containing the image data.</param>
    /// <returns>Loaded bitmap image.</returns>
    public static BitmapImage CreateBitmapImage(Stream stream)
    {
        var memoryStream = default(MemoryStream);

        // BitmapImage requires that the stream can be seekable in order
        // to be able to freeze the bitmap.

        if (!stream.CanSeek)
        {
            memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            memoryStream.Position = 0;
        }

        var bitmapImage = new BitmapImage();

        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = memoryStream ?? stream;
        bitmapImage.EndInit();

        bitmapImage.Freeze();

        memoryStream?.Dispose();

        return bitmapImage;
    }

    /// <summary>
    /// Loads an SVG image.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method supports loading a wide range of SVG images and
    /// is the preferred way to load match icons. The implementation
    /// is backed by the <a href="https://www.nuget.org/packages/SharpVectors">SharpVectors</a> NuGet package.
    /// </para>
    ///
    /// <para>
    /// The fill and stroke can be used to override the colors defined
    /// in the SVG image. This can be useful when you want to have a
    /// single SVG image for light and dark mode.
    /// </para>
    /// </remarks>
    /// <param name="stream">Stream containing the SVG data.</param>
    /// <param name="fill">Brush to override the fill color defined in the image.</param>
    /// <param name="stroke">Brush to override the stroke color defined in the image.</param>
    /// <returns></returns>
    public static DrawingImage CreateSvgImage(
        Stream stream,
        Brush? fill = null,
        Brush? stroke = null
    )
    {
        var settings = new WpfDrawingSettings
        {
            IncludeRuntime = true,
            TextAsGeometry = false,
            OptimizePath = true
        };

        using var reader = new FileSvgReader(settings);

        var drawingGroup = reader.Read(stream);
        if (drawingGroup != null)
        {
            if (fill != null || stroke != null)
                OverrideBrushes(drawingGroup, fill, stroke);

            var drawing = new DrawingImage(drawingGroup);

            drawing.Freeze();

            return drawing;
        }

        throw new InvalidOperationException("Could not convert SVG image");
    }

    private static void OverrideBrushes(DrawingGroup group, Brush? fill, Brush? stroke)
    {
        foreach (var child in group.Children)
        {
            switch (child)
            {
                case DrawingGroup drawingGroup:
                    OverrideBrushes(drawingGroup, fill, stroke);
                    break;

                case GeometryDrawing geometryDrawing:
                    if (fill != null && geometryDrawing.Brush != null)
                        geometryDrawing.Brush = fill;
                    if (stroke != null && geometryDrawing.Pen != null)
                        geometryDrawing.Pen.Brush = stroke;
                    break;
            }
        }
    }

    /// <summary>
    /// Loads an image from an embedded resource.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method loads images from embedded resources in your plugin.
    /// A pattern that can be used with this method is to have a static
    /// <c>Images</c> class in your plugin, next to a <c>Resources</c>
    /// folder containing images. The Azure plugin e.g. has this class:
    /// </para>
    ///
    /// <code><![CDATA[
    /// internal static class Images
    /// {
    ///     public static readonly ImageSource Azure = ImageFactory.FromEmbeddedResource(
    ///         typeof(Images),
    ///         "Resources.Azure.svg"
    ///     );
    /// }
    /// ]]></code>
    ///
    /// <para>
    /// Next to this class sits a folder called <c>Resources</c> with an
    /// image <c>Azure.svg</c> in it. This method uses the extension
    /// of the image to differentiate SVG images from bitmap images.
    /// </para>
    ///
    /// <para>
    /// The type parameter is used to specify both the assembly to load
    /// the resources from, and the namespace the resources are located in.
    /// </para>
    ///
    /// <para>
    /// Note that when resources are located in a folder, slashes
    /// must be replaced by dots. This is how embedded resources are
    /// stored in assemblies.
    /// </para>
    /// </remarks>
    /// <param name="relativeTo">Type relative to which the embedded
    /// resources must be loaded.</param>
    /// <param name="name">Name of the embedded resource.</param>
    /// <returns>Loaded image.</returns>
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
