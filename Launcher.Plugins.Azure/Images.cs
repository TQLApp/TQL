using Launcher.Abstractions;
using Path = System.IO.Path;

namespace Launcher.Plugins.Azure;

internal class Images
{
    public IImage Azure { get; }

    public Images(IImageFactory imageFactory)
    {
        Azure = LoadImage("Azure.svg");

        IImage LoadImage(string name)
        {
            using var stream = GetType().Assembly.GetManifestResourceStream(
                $"{GetType().Namespace}.Resources.{name}"
            );

            var imageType = Path.GetExtension(name) switch
            {
                ".svg" => ImageType.Svg,
                ".png" => ImageType.Png,
                _ => throw new InvalidOperationException("Cannot map resource extension")
            };

            return imageFactory.FromStream(stream!, imageType);
        }
    }
}
