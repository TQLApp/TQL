using Tql.App.ConfigurationUI;
using Tql.App.Search;
using Tql.App.Services;
using Tql.App.Support;
using Tql.Utilities;

namespace Tql.App.QuickStart;

internal partial class QuickStartScript
{
    private static readonly PluginValues JiraPluginValues =
        new(
            "JIRA",
            "TQLApp.Plugins.Jira",
            Guid.Parse("18760188-f7b1-448d-94ba-646b85b55d98"),
            Guid.Parse("97f80138-0b44-4ffc-b5c2-2a40f1070e17"),
            Guid.Parse("b984b4ae-0a4d-4b77-bc95-db675a6c96e9"),
            "JIRA Board",
            "board",
            "Tql.Plugins.Jira.ConfigurationUI.ConnectionEditWindow"
        );
    private static readonly PluginValues AzureDevOpsPluginValues =
        new(
            "Azure DevOps",
            "TQLApp.Plugins.AzureDevOps",
            Guid.Parse("36828080-c1f0-4759-8dff-2b764a44b62e"),
            Guid.Parse("12e42adb-7f02-40fe-b8ba-2938b49b3d81"),
            Guid.Parse("8d85fc68-ac7c-4d25-a837-7ab475b073f6"),
            "Azure Board",
            "board",
            "Tql.Plugins.AzureDevOps.ConfigurationUI.ConnectionEditWindow"
        );
    private static readonly PluginValues GitHubPluginValues =
        new(
            "GitHub",
            "TQLApp.Plugins.GitHub",
            Guid.Parse("028ffb5f-5d9f-4ee1-91fd-47f192d16e20"),
            Guid.Parse("35954273-99ae-473c-9386-2dc220a12c45"),
            Guid.Parse("d4d42f26-f777-46c5-895b-b18287fd6fb9"),
            "My GitHub Repository",
            "repository",
            "Tql.Plugins.AzureDevOps.GitHub.ConnectionEditWindow"
        );

    public void HandleMainWindow(MainWindow window)
    {
        switch (State.Step)
        {
            case QuickStartStep.Welcome:
                Welcome(window);
                break;
            case QuickStartStep.SelectTool:
                SelectTool(window);
                break;
            case QuickStartStep.InstallPlugin:
                OpenConfigurationWindowToInstallPlugin(window);
                break;
            case QuickStartStep.ConfigurePlugin:
                OpenConfigurationWindowToConfigurePlugin(window);
                break;
            case QuickStartStep.ListAllCategories:
                ListAllCategories(window);
                break;
            case QuickStartStep.SearchInsideCategory:
                SearchInsideCategory(window);
                break;
            case QuickStartStep.ActivateFavorite:
                ActivateFavorite(window);
                break;
            case QuickStartStep.Completed:
                WalkthroughComplete(window);
                break;
        }
    }

    public void HandleConfigurationWindow(ConfigurationWindow window)
    {
        switch (State.Step)
        {
            case QuickStartStep.InstallPlugin:
                OpenPluginPage(window);
                break;
            case QuickStartStep.ConfigurePlugin:
                OpenPluginConfigurationPage(window);
                break;
        }
    }

    private void Welcome(MainWindow window)
    {
        var hotKey = HotKey.FromSettings(_settings).ToLabel();

        _quickStart.Show(
            window,
            CreateBuilder("welcome", hotKey)
                .WithButton(
                    "OK",
                    () =>
                    {
                        State = State with { Step = QuickStartStep.SelectTool };
                        HandleMainWindow(window);
                    }
                )
                .Build(),
            QuickStartPopupMode.Modal
        );
    }

    private void SelectTool(MainWindow window)
    {
        var completedTools = _quickStart.State.CompletedTools;

        var builder = CreateBuilder(
            completedTools.Length switch
            {
                0 => "select-tool",
                1 => "complete-more-tools",
                _ => "complete-2nd-more-tools"
            }
        );

        AddTool(QuickStartTool.JIRA, "JIRA");
        AddTool(QuickStartTool.AzureDevOps, "Azure DevOps");
        AddTool(QuickStartTool.GitHub, "GitHub");

        if (completedTools.Length > 0)
        {
            builder.WithButton("Close Tutorial", Dismiss);
        }

        _quickStart.Show(window, builder.Build(), QuickStartPopupMode.Modal);

        void ApplySelection(QuickStartTool tool)
        {
            State = State with
            {
                SelectedTool = tool,
                Step = !IsToolInstalled(tool)
                    ? QuickStartStep.InstallPlugin
                    : QuickStartStep.ConfigurePlugin
            };
            HandleMainWindow(window);
        }

        void AddTool(QuickStartTool tool, string label)
        {
            if (!completedTools.Contains(tool))
                builder.WithChoiceButton(label, () => ApplySelection(tool));
        }
    }

    private void Dismiss()
    {
        _quickStart.Close();
        _quickStart.State = QuickStartDto.Empty with { Step = QuickStartStep.Dismissed };
    }

    private void OpenConfigurationWindowToInstallPlugin(MainWindow window)
    {
        _quickStart.Show(
            window.ConfigurationImage,
            CreateBuilder("open-configuration-window-to-install").Build()
        );
    }

    private void OpenPluginPage(ConfigurationWindow window)
    {
        _quickStart.Show(window, CreateBuilder("select-plugins-page").Build());

        window.SelectedPageChanged += CreateHandler(
            () => window.SelectedPage is PluginsConfigurationControl,
            () => FindPlugin(window)
        );
    }

    private void FindPlugin(ConfigurationWindow window)
    {
        _quickStart.Show(window, CreateBuilder("find-plugin", CurrentPlugin.PluginName).Build());

        var page = (PluginsConfigurationControl)window.SelectedPage!;

        page.SelectedPackageChanged += CreateHandler(
            () => page.SelectedPackage?.Identity.Id == CurrentPlugin.PackageId,
            () => InstallPlugin(window, page)
        );
    }

    private void InstallPlugin(ConfigurationWindow window, PluginsConfigurationControl page)
    {
        var closer = _quickStart.Show(
            (FrameworkElement?)page.InstallButton ?? window,
            CreateBuilder("install-plugin", CurrentPlugin.PluginName).Build()
        );

        page.InstallationStarted += (_, _) => closer.Dispose();

        page.InstallationCompleted += CreateHandler(() =>
        {
            State = State with { Step = QuickStartStep.ConfigurePlugin };

            InstallCompleteRestartApplication(page);
        });
    }

    private void InstallCompleteRestartApplication(PluginsConfigurationControl page)
    {
        _quickStart.Show(page.RestartButton, CreateBuilder("complete-install-plugin").Build());
    }

    private void OpenConfigurationWindowToConfigurePlugin(MainWindow window)
    {
        _quickStart.Show(
            window.ConfigurationImage,
            CreateBuilder("open-configuration-window-to-configure", CurrentPlugin.PluginName)
                .Build()
        );
    }

    private void OpenPluginConfigurationPage(ConfigurationWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("select-plugin-configuration-page", CurrentPlugin.PluginName).Build()
        );

        window.SelectedPageChanged += CreateHandler(
            () => window.SelectedPage?.PageId == CurrentPlugin.ConfigurationPageId,
            () => ConfigurePlugin(window)
        );
    }

    private void ConfigurePlugin(ConfigurationWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("configure-plugin", CurrentPlugin.PluginName).Build()
        );

        WindowLoaded(CurrentPlugin.EditWindowType, p => AddConnection(window, p));
    }

    private void AddConnection(ConfigurationWindow window, Window editWindow)
    {
        _quickStart.Show(
            editWindow,
            CreateBuilder($"add-connection-{CurrentPlugin.PluginCode}").Build()
        );

        editWindow.Closed += CreateHandler(() =>
        {
            if (editWindow.DialogResult.GetValueOrDefault())
                AddedConnection(window);
            else
                ConfigurePlugin(window);
        });
    }

    private void AddedConnection(ConfigurationWindow window)
    {
        _quickStart.Show(window, CreateBuilder("added-connection").Build());

        window.Closed += CreateHandler(
            () => window.DialogResult == true,
            () =>
            {
                if (State.CompletedTools.Length > 0)
                    UpdateStateCompletedTool();
                else
                    State = State with { Step = QuickStartStep.ListAllCategories };
            }
        );
    }

    private void ListAllCategories(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("discover-plugin", CurrentPlugin.PluginName).Build()
        );

        window.SearchManager!.SearchCompleted += CreateHandler<SearchResultsEventArgs>(
            e => window.SearchManager!.Search == " ",
            () => TrySomeCategory(window)
        );
    }

    private void TrySomeCategory(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder(
                    "select-category",
                    CurrentPlugin.PluginName,
                    CurrentPlugin.CategoryMatchLabel
                )
                .Build()
        );

        window.MatchPushed += CreateHandler<MatchEventArgs>(
            e => e.Match.TypeId.Id == CurrentPlugin.CategoryMatchId,
            () => SearchJiraBoard(window)
        );
    }

    private void SearchJiraBoard(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder(
                    $"find-match",
                    CurrentPlugin.CategoryMatchLabel,
                    Inflector.Pluralize(CurrentPlugin.CategoryLabel),
                    CurrentPlugin.CategoryLabel
                )
                .Build()
        );

        window.SearchManager!.SearchCompleted += CreateHandler<SearchResultsEventArgs>(
            e => window.SearchManager!.Search.Length >= 2,
            () => PickSomeMatch(window)
        );
    }

    private void PickSomeMatch(MainWindow window)
    {
        var hotKey = HotKey.FromSettings(_settings).ToLabel();

        var closer = _quickStart.Show(window, CreateBuilder("activate-match", hotKey).Build());

        window.MatchActivated += CreateHandler<MatchEventArgs>(() =>
        {
            closer.Dispose();

            State = State with { Step = QuickStartStep.SearchInsideCategory };
        });
    }

    private void SearchInsideCategory(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder(
                    "nested-categories",
                    CurrentPlugin.PluginName,
                    Inflector.Pluralize(CurrentPlugin.CategoryLabel),
                    CurrentPlugin.CategoryMatchLabel
                )
                .Build()
        );

        window.MatchPushed += CreateHandler<MatchEventArgs>(
            e => e.Match.TypeId.Id == CurrentPlugin.CategoryMatchId,
            () => NestedJiraSearch(window)
        );
    }

    private void NestedJiraSearch(MainWindow window)
    {
        _quickStart.Show(window, CreateBuilder("search-nested-category").Build());

        window.MatchPushed += CreateHandler<MatchEventArgs>(() => NestedJiraSearchResults(window));
    }

    private void NestedJiraSearchResults(MainWindow window)
    {
        _quickStart.Show(window, CreateBuilder($"search-hint-{CurrentPlugin.PluginCode}").Build());

        State = State with { Step = QuickStartStep.ActivateFavorite };

        window.SearchManager!.StackChanged += CreateHandler(
            () => window.SearchManager!.Stack.Length == 0,
            () => HandleMainWindow(window)
        );
    }

    private void ActivateFavorite(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("search-in-history", CurrentPlugin.CategoryLabel).Build()
        );

        window.SearchManager!.SearchCompleted += CreateHandler<SearchResultsEventArgs>(
            e => window.SearchManager!.Search.Length >= 2,
            () => PinningItems(window)
        );
    }

    private void PinningItems(MainWindow window)
    {
        _quickStart.Show(window, CreateBuilder("pinning-items").Build());

        window.MatchPinned += CreateHandler<MatchEventArgs>(() => UnpinItems(window));
    }

    private void UnpinItems(MainWindow window)
    {
        _quickStart.Show(window, CreateBuilder("unpinning-items").Build());

        window.MatchUnpinned += CreateHandler<MatchEventArgs>(() => RemoveFavorite(window));
    }

    private void RemoveFavorite(MainWindow window)
    {
        _quickStart.Show(window, CreateBuilder("remove-favorite").Build());

        window.MatchHistoryRemoved += CreateHandler<MatchEventArgs>(
            () => MainTutorialComplete(window)
        );
    }

    private void MainTutorialComplete(MainWindow window)
    {
        // The main tutorial is complete now. If the user hasn't gone through
        // all tools yet, go back to tool selection.

        UpdateStateCompletedTool();

        HandleMainWindow(window);
    }

    private void UpdateStateCompletedTool()
    {
        var completedTools = _quickStart
            .State
            .CompletedTools
            .Add(_quickStart.State.SelectedTool!.Value);

        var toolsAvailable = EnumEx
            .GetValues<QuickStartTool>()
            .Any(p => !completedTools.Contains(p));

        _quickStart.State = new QuickStartDto(
            toolsAvailable ? QuickStartStep.SelectTool : QuickStartStep.Completed,
            null,
            completedTools
        );
    }

    private void WalkthroughComplete(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("complete").WithButton("Close Tutorial", Dismiss).Build()
        );
    }
}
