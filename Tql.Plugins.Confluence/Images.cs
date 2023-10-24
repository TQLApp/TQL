using Tql.Utilities;

namespace Tql.Plugins.Confluence;

internal static class Images
{
    public static readonly ImageSource Confluence = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Confluence.svg"
    );
}
