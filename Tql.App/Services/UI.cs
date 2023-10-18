using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;

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
}
