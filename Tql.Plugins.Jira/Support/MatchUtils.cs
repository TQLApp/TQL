﻿using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Support;

internal static class MatchUtils
{
    public static string GetMatchLabel(string label, ConnectionManager connectionManager, Guid id)
    {
        if (connectionManager.Connections.Length > 1)
        {
            var connection = connectionManager.Connections.Single(p => p.Id == id);

            return $"{label} ({connection.Name})";
        }

        return label;
    }
}
