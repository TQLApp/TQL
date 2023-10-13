using Launcher.Utilities;

namespace Launcher.Plugins.GitHub;

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
}
