using System.IO;
using System.Xml.Linq;
using Launcher.Abstractions;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;

namespace Launcher.App.Services;

internal class ImageFactory : IImageFactory
{
    private static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";

    public IImage FromBytes(byte[] bytes, ImageType imageType)
    {
        using var stream = new MemoryStream(bytes);

        return FromStream(stream, imageType);
    }

    public IImage FromStream(Stream stream, ImageType imageType)
    {
        switch (imageType)
        {
            case ImageType.Png:
                return new Image(CreateBitmapImage(stream));

            case ImageType.Svg:
                return new Image(CreateSvgImage(stream));

            default:
                throw new ArgumentOutOfRangeException(nameof(imageType), imageType, null);
        }
    }

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

    public static DrawingImage CreateSvgImage(Stream stream, Brush? fill = null)
    {
        var doc = XDocument.Load(stream);

        if (doc.Root?.Elements().Count() == 1)
        {
            var element = doc.Root.Elements().Single();

            if (element.Name == SvgNs + "path")
            {
                var pathData = element.Attribute("d")?.Value;

                if (!string.IsNullOrEmpty(pathData))
                {
                    return new DrawingImage
                    {
                        Drawing = new GeometryDrawing(
                            fill ?? Brushes.White,
                            new Pen(Brushes.Transparent, 0),
                            Geometry.Parse(pathData)
                        )
                    };
                }
            }
        }

        throw new InvalidOperationException("Cannot parse SVG file");
    }
}
