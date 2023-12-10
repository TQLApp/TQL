using Tql.App.ConfigurationUI;
using Tql.App.Search;
using Tql.App.Services;
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
            new PluginMatchType(
                Guid.Parse("2370f0ee-20d6-4d0c-8da0-2430449d395b"),
                "JIRA Project",
                "project",
                "projects"
            ),
            new PluginMatchType(
                Guid.Parse("b984b4ae-0a4d-4b77-bc95-db675a6c96e9"),
                "JIRA Board",
                "board",
                "boards"
            ),
            "Tql.Plugins.Jira.ConfigurationUI.ConnectionEditWindow"
        );

    private static readonly PluginValues AzureDevOpsPluginValues =
        new(
            "Azure DevOps",
            "TQLApp.Plugins.AzureDevOps",
            Guid.Parse("36828080-c1f0-4759-8dff-2b764a44b62e"),
            Guid.Parse("12e42adb-7f02-40fe-b8ba-2938b49b3d81"),
            new PluginMatchType(
                Guid.Parse("3d57f05c-fbdd-4383-b305-3f48b2f018d2"),
                "Azure DevOps Work Item",
                "work item",
                "work items"
            ),
            new PluginMatchType(
                Guid.Parse("8d85fc68-ac7c-4d25-a837-7ab475b073f6"),
                "Azure DevOps Board",
                "board",
                "boards"
            ),
            "Tql.Plugins.AzureDevOps.ConfigurationUI.ConnectionEditWindow"
        );

    private static readonly PluginValues GitHubPluginValues =
        new(
            "GitHub",
            "TQLApp.Plugins.GitHub",
            Guid.Parse("028ffb5f-5d9f-4ee1-91fd-47f192d16e20"),
            Guid.Parse("35954273-99ae-473c-9386-2dc220a12c45"),
            new PluginMatchType(
                Guid.Parse("7ff14022-2f3d-486e-9ccd-77051c47e3db"),
                "My GitHub Issue",
                "issue",
                "issues"
            ),
            new PluginMatchType(
                Guid.Parse("d4d42f26-f777-46c5-895b-b18287fd6fb9"),
                "My GitHub Repository",
                "repository",
                "repositories"
            ),
            "Tql.Plugins.AzureDevOps.GitHub.ConnectionEditWindow"
        );

    public void HandleMainWindow(MainWindow window)
    {
        InvalidateHandlers();

        switch (State.Step)
        {
            case QuickStartStep.Welcome:
                WelcomeInstructions(window);
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
            case QuickStartStep.UsingTheApp:
                UsingTheApp(window);
                break;
            case QuickStartStep.SearchInsideCategory:
                NestedCategories(window);
                break;
            case QuickStartStep.ActivateFavorite:
                SearchInHistory(window);
                break;
            case QuickStartStep.Completed:
                WalkthroughComplete(window);
                break;
        }
    }

    public void HandleConfigurationWindow(ConfigurationWindow window)
    {
        InvalidateHandlers();

        switch (State.Step)
        {
            case QuickStartStep.InstallPlugin:
                SelectPluginsPage(window);
                break;
            case QuickStartStep.ConfigurePlugin:
                OpenPluginConfigurationPage(window);
                break;
        }
    }

    private void WelcomeInstructions(MainWindow window)
    {
        var hotKey = HotKey.FromSettings(_settings).ToLabel();

        _quickStart.Show(
            window,
            CreateBuilder("welcome-instructions", hotKey)
                .WithButton("Next", true, () => WelcomeScope(window))
                .Build(),
            QuickStartPopupMode.Modal
        );
    }

    private void WelcomeScope(MainWindow window)
    {
        var hotKey = HotKey.FromSettings(_settings).ToLabel();

        _quickStart.Show(
            window,
            CreateBuilder("welcome-scope", hotKey)
                .WithButton("Next", true, () => WelcomeOpenApp(window))
                .WithBack(() => WelcomeInstructions(window))
                .Build(),
            QuickStartPopupMode.Modal
        );
    }

    private void WelcomeOpenApp(MainWindow window)
    {
        var hotKey = HotKey.FromSettings(_settings).ToLabel();

        _quickStart.Show(
            window,
            CreateBuilder("welcome-open-app", hotKey)
                .WithButton("Next", true, () => SelectTool(window))
                .WithBack(() => WelcomeScope(window))
                .Build(),
            QuickStartPopupMode.Modal
        );
    }

    private void SelectTool(MainWindow window)
    {
        SetCurrentStep(QuickStartStep.SelectTool);

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
            builder.WithButton("Close Tutorial", true, Dismiss);
        else
            builder.WithBack(() => WelcomeOpenApp(window));

        _quickStart.Show(window, builder.Build(), QuickStartPopupMode.Modal);

        void ApplySelection(QuickStartTool tool)
        {
            State = State with { SelectedTool = tool };

            if (!IsToolInstalled(tool))
                OpenConfigurationWindowToInstallPlugin(window);
            else
                OpenConfigurationWindowToConfigurePlugin(window);
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
        SetCurrentStep(QuickStartStep.InstallPlugin);

        _quickStart.Show(
            window.ConfigurationImage,
            CreateBuilder("open-configuration-window-to-install")
                .WithBack(() => SelectTool(window))
                .Build()
        );
    }

    private void SelectPluginsPage(ConfigurationWindow window)
    {
        _quickStart.Show(window, CreateBuilder("select-plugins-page").Build());

        window.SelectedPageChanged += CreateHandler(
            () => window.SelectedPage is PluginsConfigurationControl,
            () => FindPlugin(window)
        );
    }

    private void FindPlugin(ConfigurationWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("find-plugin", CurrentPlugin.PluginName)
                .WithBack(() => SelectPluginsPage(window))
                .Build()
        );

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
            CreateBuilder("install-plugin", CurrentPlugin.PluginName)
                .WithBack(() => FindPlugin(window))
                .Build()
        );

        page.InstallationStarted += (_, _) => closer.Dispose();

        page.InstallationCompleted += CreateHandler(() => InstallCompleteRestartApplication(page));
    }

    private void InstallCompleteRestartApplication(PluginsConfigurationControl page)
    {
        SetCurrentStep(QuickStartStep.ConfigurePlugin);

        _quickStart.Show(page.RestartButton, CreateBuilder("complete-install-plugin").Build());
    }

    private void OpenConfigurationWindowToConfigurePlugin(MainWindow window)
    {
        SetCurrentStep(QuickStartStep.ConfigurePlugin);

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
            CreateBuilder("configure-plugin", CurrentPlugin.PluginName)
                .WithBack(() => OpenPluginConfigurationPage(window))
                .Build()
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
        _quickStart.Show(
            window,
            CreateBuilder("added-connection").WithBack(() => ConfigurePlugin(window)).Build()
        );

        window.Closed += CreateHandler(
            () => window.DialogResult == true,
            () =>
            {
                if (State.CompletedTools.Length > 0)
                    UpdateStateCompletedTool();
                else
                    SetCurrentStep(QuickStartStep.UsingTheApp);
            }
        );
    }

    private void UsingTheApp(MainWindow window)
    {
        SetCurrentStep(QuickStartStep.UsingTheApp);

        _quickStart.Show(
            window,
            CreateBuilder("using-the-app", CurrentPlugin.PluginName)
                .WithButton("Next", true, () => ListAllPlugins(window))
                .WithBack(() => OpenConfigurationWindowToConfigurePlugin(window))
                .Build(),
            QuickStartPopupMode.Modal
        );
    }

    private void ListAllPlugins(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("list-all-categories").WithBack(() => UsingTheApp(window)).Build()
        );

        window.SearchManager!.SearchCompleted += CreateHandler<SearchResultsEventArgs>(
            _ => window.SearchManager!.Search == " ",
            () => SelectCategory(window)
        );
    }

    private void SelectCategory(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder(
                    "select-category",
                    CurrentPlugin.PluginName,
                    CurrentPlugin.SimpleCategory.MatchLabel
                )
                .WithBack(() => ListAllPlugins(window))
                .Build()
        );

        window.SelectedMatchChanged += CreateHandler<MatchEventArgs>(
            e => e.Match.TypeId.Id == CurrentPlugin.SimpleCategory.MatchId,
            () => EnterCategory(window)
        );
    }

    private void EnterCategory(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("enter-category", CurrentPlugin.SimpleCategory.MatchLabel)
                .WithBack(() => SelectCategory(window))
                .Build()
        );

        window.MatchPushed += CreateHandler<MatchEventArgs>(
            e => e.Match.TypeId.Id == CurrentPlugin.SimpleCategory.MatchId,
            () => FindMatch(window)
        );
    }

    private void FindMatch(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder(
                    "find-match",
                    CurrentPlugin.SimpleCategory.MatchLabel,
                    CurrentPlugin.SimpleCategory.PluralLabel,
                    CurrentPlugin.SimpleCategory.SingularLabel
                )
                .WithBack(() => EnterCategory(window))
                .Build()
        );

        window.SearchManager!.SearchCompleted += CreateHandler<SearchResultsEventArgs>(
            _ => window.SearchManager!.Search.Length >= 2,
            () => ActivateMatch(window)
        );
    }

    private void ActivateMatch(MainWindow window)
    {
        var hotKey = HotKey.FromSettings(_settings).ToLabel();

        var closer = _quickStart.Show(
            window,
            CreateBuilder("activate-match", hotKey).WithBack(() => FindMatch(window)).Build()
        );

        window.MatchActivated += CreateHandler<MatchEventArgs>(() =>
        {
            closer.Dispose();

            SetCurrentStep(QuickStartStep.SearchInsideCategory);
        });
    }

    private void NestedCategories(MainWindow window)
    {
        SetCurrentStep(QuickStartStep.SearchInsideCategory);

        _quickStart.Show(
            window,
            CreateBuilder("nested-categories", CurrentPlugin.NestedCategory.MatchLabel)
                .WithBack(() => ActivateMatch(window))
                .Build()
        );

        window.MatchPushed += CreateHandler<MatchEventArgs>(
            e => e.Match.TypeId.Id == CurrentPlugin.NestedCategory.MatchId,
            () => SearchNestedCategory(window)
        );
    }

    private void SearchNestedCategory(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("search-nested-category").WithBack(() => NestedCategories(window)).Build()
        );

        window.MatchPushed += CreateHandler<MatchEventArgs>(() => SearchHint(window));
    }

    private void SearchHint(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder($"search-hint-{CurrentPlugin.PluginCode}")
                .WithBack(() => SearchNestedCategory(window))
                .Build()
        );

        window.SearchManager!.StackChanged += CreateHandler(
            () => window.SearchManager!.Stack.Length == 0,
            () => SearchInHistory(window)
        );
    }

    private void SearchInHistory(MainWindow window)
    {
        SetCurrentStep(QuickStartStep.ActivateFavorite);

        _quickStart.Show(
            window,
            CreateBuilder("search-in-history", CurrentPlugin.NestedCategory.MatchLabel)
                .WithBack(() => SearchHint(window))
                .Build()
        );

        window.SearchManager!.SearchCompleted += CreateHandler<SearchResultsEventArgs>(
            _ => window.SearchManager!.Search.Length >= 2,
            () => PinningItems(window)
        );
    }

    private void PinningItems(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("pinning-items").WithBack(() => SearchInHistory(window)).Build()
        );

        window.MatchPinned += CreateHandler<MatchEventArgs>(() => UnpinItems(window));
    }

    private void UnpinItems(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("unpinning-items").WithBack(() => PinningItems(window)).Build()
        );

        window.MatchUnpinned += CreateHandler<MatchEventArgs>(() => RemoveFavorite(window));
    }

    private void RemoveFavorite(MainWindow window)
    {
        _quickStart.Show(
            window,
            CreateBuilder("remove-favorite").WithBack(() => UnpinItems(window)).Build()
        );

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
            CreateBuilder("complete").WithButton("Close Tutorial", true, Dismiss).Build(),
            QuickStartPopupMode.Modal
        );
    }
}
