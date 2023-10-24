using Tql.Utilities;

namespace Tql.Plugins.Jira;

internal static class Images
{
    public static readonly ImageSource Boards = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Boards.svg"
    );
    public static readonly ImageSource Dashboards = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Dashboards.svg"
    );
    public static readonly ImageSource Issues = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Issues.svg"
    );
    public static readonly ImageSource Jira = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Jira.svg"
    );
    public static readonly ImageSource Projects = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Projects.svg"
    );
}
