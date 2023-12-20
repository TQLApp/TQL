using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps;

internal static class Images
{
    private static ImageSource GetImage(string name) =>
        ImageFactory.FromEmbeddedResource(typeof(Images), $"Resources.{name}");

    public static readonly ImageSource Azure = GetImage("Azure.svg");
    public static readonly ImageSource Boards = GetImage("Boards.svg");
    public static readonly ImageSource Dashboards = GetImage("Dashboards.png");
    public static readonly ImageSource Document = GetImage("Document.svg");
    public static readonly ImageSource Pipelines = GetImage("Pipelines.svg");
    public static readonly ImageSource Repositories = GetImage("Repositories.svg");
    public static readonly ImageSource PullRequest = GetImage("Pull Request.svg");
}
