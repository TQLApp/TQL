﻿using System.Windows.Threading;
using Tql.App.Services;

namespace Tql.App.QuickStart;

internal partial class QuickStartScript
{
    private readonly IPlaybook _playbook = LoadPlaybook();
    private readonly QuickStartManager _quickStart;
    private readonly IPluginManager _pluginManager;
    private readonly Settings _settings;
    private readonly List<(string TypeName, Action<Window> Action)> _windowLoadedListeners = new();
    private int _cookie;

    public QuickStartScript(
        QuickStartManager quickStart,
        IPluginManager pluginManager,
        Settings settings
    )
    {
        _quickStart = quickStart;
        _pluginManager = pluginManager;
        _settings = settings;

        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnWindowLoaded)
        );
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (_windowLoadedListeners.Count == 0)
            return;

        var window = (Window)sender;
        var typeName = window.GetType().FullName;

        foreach (var listener in _windowLoadedListeners.Where(p => p.TypeName == typeName).ToList())
        {
            _windowLoadedListeners.Remove(listener);

            window.Dispatcher.BeginInvoke(() => listener.Action(window), DispatcherPriority.Loaded);
        }
    }

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
        get => _quickStart.State;
        set => _quickStart.State = value;
    }

    private bool IsToolInstalled(QuickStartTool tool)
    {
        var plugin = GetPluginValues(tool);

        return _pluginManager.Plugins.Any(p => p.Id == plugin.Id);
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

    private void InvalidateHandlers()
    {
        // Invalidate any attached handlers.
        _cookie++;
    }

    private EventHandler CreateHandler(Action action)
    {
        var cookie = ++_cookie;

        return (_, _) =>
        {
            if (cookie == _cookie)
            {
                cookie = -1;
                action();
            }
        };
    }

    private EventHandler<T> CreateHandler<T>(Action action)
        where T : EventArgs
    {
        var cookie = ++_cookie;

        return (_, _) =>
        {
            if (cookie == _cookie)
            {
                cookie = -1;
                action();
            }
        };
    }

    private EventHandler CreateHandler(Func<bool> condition, Action action)
    {
        var cookie = ++_cookie;

        return (_, _) =>
        {
            if (cookie == _cookie && condition())
            {
                cookie = -1;
                action();
            }
        };
    }

    private EventHandler<T> CreateHandler<T>(Func<T, bool> condition, Action action)
        where T : EventArgs
    {
        var cookie = ++_cookie;

        return (_, e) =>
        {
            if (cookie == _cookie && condition(e))
            {
                cookie = -1;
                action();
            }
        };
    }

    private void WindowLoaded(string typeName, Action<Window> action)
    {
        _windowLoadedListeners.Add((typeName, action));
    }

    private void SetCurrentStep(QuickStartStep step)
    {
        State = State with { Step = step };
    }

    private record PluginValues(
        string PluginName,
        string PackageId,
        Guid Id,
        Guid ConfigurationPageId,
        PluginMatchType SimpleCategory,
        PluginMatchType NestedCategory,
        string EditWindowType
    )
    {
        public string PluginCode => PluginName.ToLowerInvariant().Replace(" ", "-");
    }

    private record PluginMatchType(
        Guid MatchId,
        string MatchLabel,
        string SingularLabel,
        string PluralLabel
    );
}
