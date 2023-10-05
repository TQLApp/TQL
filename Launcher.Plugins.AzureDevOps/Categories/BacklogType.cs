﻿using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class BacklogType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.Backlog.Id;

    public BacklogType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new BacklogMatch(JsonSerializer.Deserialize<BacklogMatchDto>(json)!, _images);
    }
}
