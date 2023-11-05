namespace Tql.App.Support;

internal static class WpfUtils
{
    private const int NoiseTextureSize = 128;

    public static double PointsToPixels(int points)
    {
        return (points / 72.0) * 96.0;
    }

    public static Brush CreateAcrylicBrush(Window window)
    {
        var dpiScale = VisualTreeHelper.GetDpi(window);

        var dpiX = dpiScale.PixelsPerInchX;
        var dpiY = dpiScale.PixelsPerInchY;

        var width = (int)(NoiseTextureSize * dpiX / 96);
        var height = (int)(NoiseTextureSize * dpiY / 96);

        var bitmap = new WriteableBitmap(width, height, dpiX, dpiY, PixelFormats.Gray8, null);
        var pixels = new byte[width * height];

        new Random().NextBytes(pixels);

        bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width, 0);

        return new ImageBrush
        {
            ImageSource = bitmap,
            TileMode = TileMode.Tile,
            ViewportUnits = BrushMappingMode.Absolute,
            Viewport = new Rect(0, 0, 128, 128),
            Opacity = 0.03
        };
    }
}
