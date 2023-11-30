using System.IO;

namespace Tql.App.Support;

internal static class CursorUtils
{
    // Taken from https://stackoverflow.com/questions/35296030.
    public static Cursor Create(DrawingImage image, Point hotspot, Size cursorSize, DpiScale scale)
    {
        var scaledSize = new Size(
            cursorSize.Width * scale.DpiScaleX,
            cursorSize.Height * scale.DpiScaleY
        );
        var scaledHotspot = new Point(hotspot.X * scale.DpiScaleX, hotspot.Y * scale.DpiScaleY);

        var vis = new DrawingVisual();
        using (var dc = vis.RenderOpen())
        {
            dc.DrawImage(image, new Rect(0, 0, scaledSize.Width, scaledSize.Height));
            dc.Close();
        }
        var rtb = new RenderTargetBitmap(
            (int)scaledSize.Width,
            (int)scaledSize.Height,
            96,
            96,
            PixelFormats.Pbgra32
        );
        rtb.Render(vis);

        using (var ms1 = new MemoryStream())
        {
            var penc = new PngBitmapEncoder();
            penc.Frames.Add(BitmapFrame.Create(rtb));
            penc.Save(ms1);

            var pngBytes = ms1.ToArray();
            var size = pngBytes.GetLength(0);

            //.cur format spec http://en.wikipedia.org/wiki/ICO_(file_format)
            using (var ms = new MemoryStream())
            {
                { //ICONDIR Structure
                    ms.Write(BitConverter.GetBytes((Int16)0), 0, 2); //Reserved must be zero; 2 bytes
                    ms.Write(BitConverter.GetBytes((Int16)2), 0, 2); //image type 1 = ico 2 = cur; 2 bytes
                    ms.Write(BitConverter.GetBytes((Int16)1), 0, 2); //number of images; 2 bytes
                }

                { //ICONDIRENTRY structure
                    ms.WriteByte((byte)(scaledSize.Width)); //image width in pixels
                    ms.WriteByte((byte)(scaledSize.Height)); //image height in pixels

                    ms.WriteByte(0); //Number of Colors in the color palette. Should be 0 if the image doesn't use a color palette
                    ms.WriteByte(0); //reserved must be 0

                    ms.Write(BitConverter.GetBytes((Int16)(scaledHotspot.X / 2.0)), 0, 2); //2 bytes. In CUR format: Specifies the horizontal coordinates of the hotspot in number of pixels from the left.
                    ms.Write(BitConverter.GetBytes((Int16)(scaledHotspot.Y / 2.0)), 0, 2); //2 bytes. In CUR format: Specifies the vertical coordinates of the hotspot in number of pixels from the top.

                    ms.Write(BitConverter.GetBytes(size), 0, 4); //Specifies the size of the image's data in bytes
                    ms.Write(BitConverter.GetBytes((Int32)22), 0, 4); //Specifies the offset of BMP or PNG data from the beginning of the ICO/CUR file
                }

                ms.Write(pngBytes, 0, size); //write the png data.
                ms.Seek(0, SeekOrigin.Begin);
                return new Cursor(ms);
            }
        }
    }
}
