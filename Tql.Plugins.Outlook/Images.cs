using Tql.Utilities;

namespace Tql.Plugins.Outlook;

internal static class Images
{
    public static readonly ImageSource Outlook = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Outlook.svg"
    );
}
