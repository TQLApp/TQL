using Launcher.Abstractions;

namespace Launcher.Plugins.AzureDevOps;

internal class Images
{
    public IImage Boards { get; }
    public IImage Dashboards { get; }
    public IImage Pipelines { get; }
    public IImage Repositories { get; }

    public Images(IImageFactory imageFactory)
    {
        Boards = LoadImage("Boards");
        Dashboards = LoadImage("Dashboards");
        Pipelines = LoadImage("Pipelines");
        Repositories = LoadImage("Repositories");

        IImage LoadImage(string name)
        {
            using var stream = GetType().Assembly.GetManifestResourceStream(
                $"{GetType().Namespace}.Resources.{name}.png"
            );

            return imageFactory.FromStream(stream!);
        }
    }
}
