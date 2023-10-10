using System.IO;
using Launcher.Abstractions;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace Launcher.App.Services;

internal class ImageFactory : IImageFactory
{
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

    public IImage FromImageSource(ImageSource imageSource)
    {
        return new Image(imageSource);
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
}
