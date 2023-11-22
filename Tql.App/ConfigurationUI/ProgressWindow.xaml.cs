using System.Runtime.ExceptionServices;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.App.Services;
using Tql.App.Support;

namespace Tql.App.ConfigurationUI;

internal partial class ProgressWindow
{
    private readonly InMemoryLoggerProvider _loggerProvider;
    private readonly Action _action;
    private bool _complete;
    private readonly DispatcherTimer _timer;
    private readonly StringBuilder _sb = new();
    private readonly object _syncRoot = new();
    private ExceptionDispatchInfo? _exception;

    public static void Show(IServiceProvider serviceProvider, UIElement owner, Func<Task> action)
    {
        Show(serviceProvider, owner, () => TaskUtils.RunSynchronously(action));
    }

    public static void Show(IServiceProvider serviceProvider, UIElement owner, Action action)
    {
        var loggerProvider = serviceProvider.GetRequiredService<InMemoryLoggerProvider>();
        var ui = (UI)serviceProvider.GetRequiredService<IUI>();

        var window = new ProgressWindow(loggerProvider, action) { Owner = GetWindow(owner) };

        ui.EnterModalDialog();
        try
        {
            window.ShowDialog();
        }
        finally
        {
            ui.ExitModalDialog();
        }

        window._exception?.Throw();
    }

    private ProgressWindow(InMemoryLoggerProvider loggerProvider, Action action)
    {
        _loggerProvider = loggerProvider;
        _action = action;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
        _timer.Tick += _timer_Tick;
        _timer.Start();

        InitializeComponent();
    }

    private void _timer_Tick(object? sender, EventArgs e)
    {
        string log;

        lock (_syncRoot)
        {
            if (_sb.Length == 0)
                return;
            log = _sb.ToString();
            _sb.Clear();
        }

        _log.AppendText(log);
        _log.ScrollToEnd();
    }

    private void LoggerProvider_Logged(object? sender, InMemoryLogMessageEventArgs e)
    {
        lock (_syncRoot)
        {
            _sb.AppendLine(
                $"[{e.Message.LogLevel}] {e.Message.CategoryName} - {e.Message.Message}"
            );
        }
    }

    private void Window_Loaded(object? sender, RoutedEventArgs e)
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
            _exception = ExceptionDispatchInfo.Capture(ex);
        }
        finally
        {
            Dispatcher.BeginInvoke(() =>
            {
                _timer.Stop();
                _complete = true;
                Close();
            });
        }
    }

    private void Window_Unloaded(object? sender, RoutedEventArgs e)
    {
        _loggerProvider.Logged -= LoggerProvider_Logged;
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = !_complete;
    }
}
