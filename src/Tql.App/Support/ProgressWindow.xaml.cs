using System.ComponentModel;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.App.Services;

namespace Tql.App.Support;

internal partial class ProgressWindow
{
    private readonly Action<IProgress> _action;
    private bool _complete;
    private ExceptionDispatchInfo? _exception;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public static void Show(
        IServiceProvider serviceProvider,
        UIElement owner,
        Func<IProgress, Task> action
    )
    {
        Show(serviceProvider, owner, p => TaskUtils.RunSynchronously(() => action(p)));
    }

    public static void Show(
        IServiceProvider serviceProvider,
        UIElement owner,
        Action<IProgress> action
    )
    {
        var ui = (UI)serviceProvider.GetRequiredService<IUI>();

        var window = new ProgressWindow(action, serviceProvider) { Owner = GetWindow(owner) };

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

    private ProgressWindow(Action<IProgress> action, IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _action = action;

        InitializeComponent();
    }

    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        var thread = new Thread(ThreadProc);

        thread.Start(new Progress(this));
    }

    private void ThreadProc(object? obj)
    {
        try
        {
            _action((Progress)obj!);
        }
        catch (Exception ex)
        {
            _exception = ExceptionDispatchInfo.Capture(ex);
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

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_cancelButton.IsEnabled)
            _cancellationTokenSource.Cancel();

        e.Cancel = !_complete;
    }

    private void SetProgress(string? status, double progress)
    {
        Dispatcher.BeginInvoke(() =>
        {
            if (status != null)
                _status.Text = status;

            _progress.Value = Math.Clamp(progress, _progress.Minimum, _progress.Maximum);
        });
    }

    private void SetCanCancel(bool value)
    {
        Dispatcher.BeginInvoke(() => _cancelButton.IsEnabled = value);
    }

    private void _cancelButton_Click(object sender, RoutedEventArgs e)
    {
        _cancellationTokenSource.Cancel();
        _cancelButton.IsEnabled = false;
    }

    private class Progress(ProgressWindow owner) : IProgress
    {
        private bool _canCancel = owner._cancelButton.IsEnabled;

        public bool CanCancel
        {
            get => _canCancel;
            set
            {
                if (_canCancel != value)
                {
                    _canCancel = value;
                    owner.SetCanCancel(value);
                }
            }
        }

        public CancellationToken CancellationToken { get; } = owner._cancellationTokenSource.Token;

        public void SetProgress(double progress) => SetProgress(null, progress);

        public void SetProgress(string? status, double progress) =>
            owner.SetProgress(status, progress);
    }
}
