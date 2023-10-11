using Launcher.Abstractions;
using Path = System.IO.Path;

namespace Launcher.Plugins.AzureDevOps;

internal class Images
{
    public IImage Azure { get; }
    public IImage Boards { get; }
    public IImage Dashboards { get; }
    public IImage Document { get; }
    public IImage Outlook { get; }
    public IImage Pipelines { get; }
    public IImage Repositories { get; }
    public IImage Teams { get; }

    public Images(IImageFactory imageFactory)
    {
        Azure = LoadImage("Azure.svg");
        Boards = LoadImage("Boards.png");
        Dashboards = LoadImage("Dashboards.png");
        Document = LoadImage("Document.svg");
        Outlook = LoadImage("Outlook.svg");
        Pipelines = LoadImage("Pipelines.png");
        Repositories = LoadImage("Repositories.png");
        Teams = LoadImage("Teams.svg");

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
