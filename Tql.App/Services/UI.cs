using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Support;
using Tql.Interop;
using Application = System.Windows.Application;

namespace Tql.App.Services;

internal class UI : IUI
{
    private readonly ILogger<UI> _logger;
    private SynchronizationContext? _synchronizationContext;
    private MainWindow? _mainWindow;

    public UI(ILogger<UI> logger)
    {
        _logger = logger;
    }

    public void SetSynchronizationContext(SynchronizationContext synchronizationContext)
    {
        _synchronizationContext = synchronizationContext;
    }

    public Task PerformInteractiveAuthentication(
        IInteractiveAuthentication interactiveAuthentication
    )
    {
        var tcs = new TaskCompletionSource<bool>();

        _synchronizationContext!.Post(
            _ =>
            {
                var window = new InteractiveAuthenticationWindow(interactiveAuthentication)
                {
                    Owner = _mainWindow
                };

                window.ShowDialog();

                if (window.Exception != null)
                    tcs.SetException(window.Exception);
                else
                    tcs.SetResult(true);
            },
            null
        );

        return tcs.Task;
    }

    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open '{Url}'", url);
        }
    }

    public void SetMainWindow(MainWindow? mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void Shutdown()
    {
        _synchronizationContext?.Post(_ => Application.Current.Shutdown(), null);
    }

    public DialogResult ShowConfirmation(
        UIElement owner,
        string title,
        string? subtitle = null,
        DialogCommonButtons buttons = DialogCommonButtons.Yes | DialogCommonButtons.No,
        DialogIcon icon = DialogIcon.Warning
    )
    {
        return TaskDialogEx.Confirm(
            owner,
            title,
            subtitle,
            (TaskDialogCommonButtons)buttons,
            (TaskDialogIcon)icon
        );
    }

    public DialogResult ShowError(
        UIElement owner,
        string title,
        Exception exception,
        DialogIcon icon = DialogIcon.Error,
        DialogCommonButtons buttons = DialogCommonButtons.OK
    )
    {
        return TaskDialogEx.Error(
            owner,
            title,
            exception,
            (TaskDialogIcon)icon,
            (TaskDialogCommonButtons)buttons
        );
    }
}
