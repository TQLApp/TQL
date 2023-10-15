using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps;

internal static class Images
{
    public static readonly ImageSource Azure = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Azure.svg"
    );
    public static readonly ImageSource Boards = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Boards.png"
    );
    public static readonly ImageSource Dashboards = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Dashboards.png"
    );
    public static readonly ImageSource Document = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Document.svg"
    );
    public static readonly ImageSource Outlook = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Outlook.svg"
    );
    public static readonly ImageSource Pipelines = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Pipelines.png"
    );
    public static readonly ImageSource Repositories = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Repositories.png"
    );
    public static readonly ImageSource Teams = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Teams.svg"
    );
}
