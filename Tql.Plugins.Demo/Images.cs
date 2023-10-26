using Tql.Utilities;

namespace Tql.Plugins.Demo;

internal static class Images
{
    public static readonly ImageSource SpaceShuttle = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Space Shuttle.svg"
    );
}
