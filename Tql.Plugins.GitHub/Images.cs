using Tql.Utilities;

namespace Tql.Plugins.GitHub;

internal static class Images
{
    public static readonly ImageSource GitHub = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.GitHub.svg"
    );
    public static readonly ImageSource Copy = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Copy.svg"
    );
    public static readonly ImageSource OpenIssue = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Open Issue.svg"
    );
    public static readonly ImageSource ClosedIssue = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Closed Issue.svg"
    );
    public static readonly ImageSource Repository = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Repository.svg"
    );
    public static readonly ImageSource Issue = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Issue.svg"
    );
    public static readonly ImageSource PullRequest = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Pull Request.svg"
    );
    public static readonly ImageSource User = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.User.svg"
    );
    public static readonly ImageSource Gist = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Gist.svg"
    );
}
