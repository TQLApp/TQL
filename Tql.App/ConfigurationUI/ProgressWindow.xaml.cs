using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.ConfigurationUI;

internal partial class ProgressWindow
{
    private readonly InMemoryLoggerProvider _loggerProvider;
    private readonly Action _action;
    private readonly IUI _ui;
    private bool _complete;

    public static void Show(IServiceProvider serviceProvider, UIElement owner, Func<Task> action)
    {
        Show(serviceProvider, owner, () => TaskUtils.RunSynchronously(action));
    }

    public static void Show(IServiceProvider serviceProvider, UIElement owner, Action action)
    {
        var loggerProvider = serviceProvider.GetRequiredService<InMemoryLoggerProvider>();
        var ui = serviceProvider.GetRequiredService<IUI>();

        var window = new ProgressWindow(loggerProvider, action, ui) { Owner = GetWindow(owner) };

        window.ShowDialog();
    }

    private ProgressWindow(InMemoryLoggerProvider loggerProvider, Action action, IUI ui)
    {
        _loggerProvider = loggerProvider;
        _action = action;
        _ui = ui;

        InitializeComponent();
    }

    private void LoggerProvider_Logged(object sender, InMemoryLogMessageEventArgs e)
    {
        Dispatcher.BeginInvoke(() =>
        {
            _log.AppendText(
                $"[{e.Message.LogLevel}] {e.Message.CategoryName} - {e.Message.Message}"
                    + Environment.NewLine
            );
            _log.ScrollToEnd();
        });
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _loggerProvider.Logged += LoggerProvider_Logged;

        var thread = new Thread(ThreadProc);

        thread.Start();
    }

    private void ThreadProc()
    {
        try
        {
            _action();
        }
        catch (Exception ex)
        {
            Dispatcher.BeginInvoke(() => _ui.ShowError(this, "An unexpected error occurred", ex));
        }
        finally
        {
            Dispatcher.BeginInvoke(() =>
            {
                _complete = true;
                Close();
            });
        }
    }

    private void Window_Unloaded(object sender, RoutedEventArgs e)
    {
        _loggerProvider.Logged -= LoggerProvider_Logged;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = !_complete;
    }
}
