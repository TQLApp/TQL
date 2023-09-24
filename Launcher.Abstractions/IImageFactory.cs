using System.IO;

namespace Launcher.Abstractions;

public interface IImageFactory
{
    IImage FromBytes(byte[] bytes);
    IImage FromStream(Stream stream);
}
