﻿using System;
using System.Collections.Immutable;
using Tql.Plugins.Azure;
using Tql.Plugins.AzureDevOps;
using Tql.Plugins.Confluence;
using Tql.Plugins.Demo;
using Tql.Plugins.GitHub;
using Tql.Plugins.Jira;
using Tql.Plugins.MicrosoftTeams;
using Tql.Plugins.Outlook;

namespace Tql.DebugApp;

public static class Program
{
    [STAThread]
    public static int Main()
    {
        App.App.DebugAssemblies = ImmutableArray.Create(
            typeof(AzureDevOpsPlugin).Assembly,
            typeof(AzurePlugin).Assembly,
            typeof(GitHubPlugin).Assembly,
            typeof(JiraPlugin).Assembly,
            typeof(ConfluencePlugin).Assembly,
            typeof(MicrosoftTeamsPlugin).Assembly,
            typeof(OutlookPlugin).Assembly,
            typeof(DemoPlugin).Assembly
        );

        App.App.IsDebugMode = true;

        var app = new App.App();

        app.InitializeComponent();

        return app.Run();
    }
}
