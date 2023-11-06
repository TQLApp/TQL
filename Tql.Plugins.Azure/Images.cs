using Tql.Utilities;

namespace Tql.Plugins.Azure;

internal static class Images
{
    public static readonly ImageSource Azure = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Azure.svg"
    );
}
