using System.IO;

namespace Launcher.Abstractions;

public interface IImageFactory
{
    IImage FromBytes(byte[] bytes, ImageType imageType);
    IImage FromStream(Stream stream, ImageType imageType);
}
