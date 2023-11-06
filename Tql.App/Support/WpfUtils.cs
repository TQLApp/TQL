namespace Tql.App.Support;

internal static class WpfUtils
{
    private const int NoiseTextureSize = 256;

    public static double PointsToPixels(int points)
    {
        return (points / 72.0) * 96.0;
    }

    public static Brush CreateAcrylicBrush(Window window)
    {
        // See https://learn.microsoft.com/en-us/windows/apps/design/style/acrylic for info.

        var dpiScale = VisualTreeHelper.GetDpi(window);

        var dpiX = dpiScale.PixelsPerInchX;
        var dpiY = dpiScale.PixelsPerInchY;

        var width = (int)(NoiseTextureSize * dpiX / 96);
        var height = (int)(NoiseTextureSize * dpiY / 96);

        var pixels = new byte[width * height];

        new Random().NextBytes(pixels);

        var bitmap = BitmapSource.Create(
            width,
            height,
            dpiX,
            dpiY,
            PixelFormats.Gray8,
            null,
            pixels,
            width
        );

        bitmap.Freeze();

        return new ImageBrush
        {
            ImageSource = bitmap,
            TileMode = TileMode.Tile,
            ViewportUnits = BrushMappingMode.Absolute,
            Viewport = new Rect(0, 0, NoiseTextureSize, NoiseTextureSize),
            Opacity = 0.02
        };
    }
}
