﻿using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams;

internal static class Images
{
    public static readonly ImageSource Teams = ImageFactory.FromEmbeddedResource(
        typeof(Images),
        "Resources.Teams.svg"
    );
}
