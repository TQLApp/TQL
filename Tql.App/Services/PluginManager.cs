﻿using Tql.Abstractions;

namespace Tql.App.Services;

internal record PluginManager(ImmutableArray<ITqlPlugin> Plugins) : IPluginManager
{
    public void Initialize(IServiceProvider serviceProvider)
    {
        foreach (var plugin in Plugins)
        {
            plugin.Initialize(serviceProvider);
        }
    }
};