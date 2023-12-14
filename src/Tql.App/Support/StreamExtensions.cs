using System.IO;

namespace Tql.App.Support;

internal static class StreamExtensions
{
    public static ImmutableArray<byte> ToImmutableArray(this Stream self)
    {
        if (self.CanSeek)
            self.Position = 0;

        var builder = self.CanSeek
            ? ImmutableArray.CreateBuilder<byte>((int)self.Length)
            : ImmutableArray.CreateBuilder<byte>();

        var buffer = new byte[4096];
        int read;

        while ((read = self.Read(buffer, 0, buffer.Length)) > 0)
        {
            builder.AddRange(buffer.AsSpan(0, read));
        }

        return builder.DrainToImmutable();
    }
}
