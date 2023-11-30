using System.Windows.Threading;
using Tql.App.Support;

namespace Tql.App;

internal class SplashScreen : IDisposable
{
    private Thread? _thread;
    private volatile SplashScreenWindow? _window;
    private readonly object _syncRoot = new();

    public IProgress Progress { get; }

    public SplashScreen()
    {
        Progress = new ProgressImpl(this);
    }

    public void Show()
    {
        lock (_syncRoot)
        {
            if (_thread != null)
                return;

            using var @event = new ManualResetEventSlim();

            _thread = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(
                    new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher)
                );

                _window = new SplashScreenWindow();

                @event.Set();

                _window.Closed += (_, _) =>
                    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

                _window.Show();

                Dispatcher.Run();
            });

            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();

            @event.Wait();
        }
    }

    public void Hide()
    {
        lock (_syncRoot)
        {
            if (_thread == null)
                return;

            _window?.Dispatcher.BeginInvoke(() => _window?.Close());

            _thread.Join();
            _thread = null;
        }
    }

    public void Dispose()
    {
        Hide();
    }

    private class ProgressImpl(SplashScreen owner) : IProgress
    {
        public bool CanCancel
        {
            get => false;
            set { }
        }

        public CancellationToken CancellationToken => default;

        public void SetProgress(double progress)
        {
            SetProgress(null, progress);
        }

        public void SetProgress(string? status, double progress)
        {
            owner
                ._window
                ?.Dispatcher
                .BeginInvoke(() => owner._window.SetProgress(status, progress));
        }
    }
}
