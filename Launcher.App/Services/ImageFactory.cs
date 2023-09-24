using System.IO;
using Launcher.Abstractions;

namespace Launcher.App.Services;

internal class ImageFactory : IImageFactory
{
    public IImage FromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);

        return FromStream(stream);
    }

    public IImage FromStream(Stream stream)
    {
        return new Image(stream);
    }
}
