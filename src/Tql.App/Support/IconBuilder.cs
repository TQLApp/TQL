using System.Drawing;
using System.IO;

namespace Tql.App.Support;

internal static class IconBuilder
{
    private static readonly int[] Resolutions = { 16, 24, 32, 48, 256 };

    public static Icon Build(ImageSource image, params ImageSource[] overlays)
    {
        var bitmaps = Resolutions
            .Select(p => RenderImage(p, image, overlays))
            .Select(p => (Frame: p, Stream: GetBitmapStream(p)))
            .ToList();

        using var stream = new MemoryStream();

        // ICO header
        stream.Write([0, 0, 1, 0, (byte)bitmaps.Count, 0], 0, 6);

        var offset = 6L + (16 * bitmaps.Count); // Initial offset after header and directory

        foreach (var bitmap in bitmaps)
        {
            // Image entry
            stream.WriteByte((byte)bitmap.Frame.Width);
            stream.WriteByte((byte)bitmap.Frame.Height);
            stream.WriteByte(0); // No palette
            stream.WriteByte(0); // Reserved
            stream.Write(BitConverter.GetBytes((short)1), 0, 2); // Color planes
            stream.Write(BitConverter.GetBytes((short)32), 0, 2); // Bits per pixel
            stream.Write(BitConverter.GetBytes(bitmap.Stream.Length), 0, 4);
            stream.Write(BitConverter.GetBytes(offset), 0, 4);

            offset += bitmap.Stream.Length;
        }

        foreach (var bitmap in bitmaps)
        {
            bitmap.Stream.CopyTo(stream);
        }

        stream.Position = 0;

        return new Icon(stream);
    }

    private static BitmapFrame RenderImage(
        int resolution,
        ImageSource image,
        ImageSource[] overlays
    )
    {
        var vis = new DrawingVisual();

        using (var dc = vis.RenderOpen())
        {
            dc.DrawImage(image, new Rect(0, 0, resolution, resolution));

            foreach (var overlay in overlays)
            {
                dc.DrawImage(overlay, new Rect(0, 0, resolution, resolution));
            }

            dc.Close();
        }

        var rtb = new RenderTargetBitmap(resolution, resolution, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(vis);

        return BitmapFrame.Create(rtb);
    }

    private static Stream GetBitmapStream(BitmapFrame frame)
    {
        var stream = new MemoryStream();

        BitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(frame);
        encoder.Save(stream);

        stream.Position = 0;

        return stream;
    }
}
