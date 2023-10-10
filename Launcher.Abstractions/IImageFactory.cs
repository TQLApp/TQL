using System.IO;
using System.Windows.Media;

namespace Launcher.Abstractions;

public interface IImageFactory
{
    IImage FromBytes(byte[] bytes, ImageType imageType);
    IImage FromStream(Stream stream, ImageType imageType);
    IImage FromImageSource(ImageSource imageSource);
}
