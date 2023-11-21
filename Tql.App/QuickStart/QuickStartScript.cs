using Tql.App.Services;

namespace Tql.App.QuickStart;

internal partial class QuickStartScript(
    QuickStartManager quickStart,
    IPluginManager pluginManager,
    Settings settings
)
{
    private readonly IPlaybook _playbook = LoadPlaybook();

    private static IPlaybook LoadPlaybook()
    {
#if DEBUG
        return DebugPlaybook.Load();
#else
        return Playbook.Load();
#endif
    }

    private QuickStartDto State
    {
        get => quickStart.State;
        set => quickStart.State = value;
    }

    private bool IsToolInstalled(QuickStartTool tool)
    {
        var plugin = GetPluginValues(tool);

        return pluginManager.Plugins.Any(p => p.Id == plugin.Id);
    }

    private PluginValues CurrentPlugin => GetPluginValues(State.SelectedTool);

    private PluginValues GetPluginValues(QuickStartTool? tool) =>
        tool switch
        {
            QuickStartTool.JIRA => JiraPluginValues,
            QuickStartTool.AzureDevOps => AzureDevOpsPluginValues,
            QuickStartTool.GitHub => GitHubPluginValues,
            _ => throw new InvalidOperationException("No current tool selected")
        };

    private QuickStartPopupBuilder CreateBuilder(string id, params object[] args) =>
        QuickStartPopup.CreateBuilder(_playbook, id, args);

    private EventHandler CreateHandler(Action action)
    {
        var handled = false;

        return (_, _) =>
        {
            if (!handled)
            {
                handled = true;
                action();
            }
        };
    }

    private EventHandler<T> CreateHandler<T>(Action action)
        where T : EventArgs
    {
        var handled = false;

        return (_, _) =>
        {
            if (!handled)
            {
                handled = true;
                action();
            }
        };
    }

    private EventHandler CreateHandler(Func<bool> condition, Action action)
    {
        var handled = false;

        return (_, _) =>
        {
            if (!handled && condition())
            {
                handled = true;
                action();
            }
        };
    }

    private EventHandler<T> CreateHandler<T>(Func<T, bool> condition, Action action)
        where T : EventArgs
    {
        var handled = false;

        return (_, e) =>
        {
            if (!handled && condition(e))
            {
                handled = true;
                action();
            }
        };
    }

    private record PluginValues(
        string PluginName,
        string PackageId,
        Guid Id,
        Guid ConfigurationPageId,
        Guid CategoryMatchId,
        string CategoryMatchLabel,
        string CategoryLabel
    )
    {
        public string PluginCode => PluginName.ToLowerInvariant().Replace(" ", "-");
    }
}
