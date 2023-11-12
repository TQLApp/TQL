using System.Collections.Concurrent;

namespace Tql.App.Support;

internal static class WpfUtils
{
    private static readonly ConcurrentDictionary<
        (double DpiScaleX, double DpiScaleY),
        Brush
    > BrushCache = new();

    private const int NoiseTextureSize = 256;

    public static double PointsToPixels(int points)
    {
        return (points / 72.0) * 96.0;
    }

    public static Brush GetAcrylicBrush(Window window)
    {
        var dpiScale = VisualTreeHelper.GetDpi(window);

        return BrushCache.GetOrAdd((dpiScale.DpiScaleX, dpiScale.DpiScaleY), CreateAcrylicBrush);
    }

    private static Brush CreateAcrylicBrush((double DpiScaleX, double DpiScaleY) dpiScale)
    {
        // See https://learn.microsoft.com/en-us/windows/apps/design/style/acrylic for info.

        var width = (int)(NoiseTextureSize * dpiScale.DpiScaleX);
        var height = (int)(NoiseTextureSize * dpiScale.DpiScaleY);

        var pixels = new byte[width * height];

        new Random().NextBytes(pixels);

        var bitmap = BitmapSource.Create(
            width,
            height,
            dpiScale.DpiScaleX * 96,
            dpiScale.DpiScaleY * 96,
            PixelFormats.Gray8,
            null,
            pixels,
            width
        );

        bitmap.Freeze();

        var brush = new ImageBrush
        {
            ImageSource = bitmap,
            TileMode = TileMode.Tile,
            ViewportUnits = BrushMappingMode.Absolute,
            Viewport = new Rect(0, 0, NoiseTextureSize, NoiseTextureSize),
            Opacity = 0.02
        };

        brush.Freeze();

        return brush;
    }
}
