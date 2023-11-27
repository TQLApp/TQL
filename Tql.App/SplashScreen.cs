using System.Windows.Threading;

namespace Tql.App;

internal class SplashScreen : IDisposable
{
    private readonly Thread _thread;
    private volatile SplashScreenWindow? _window;

    public SplashScreen()
    {
        using var @event = new ManualResetEventSlim();

        _thread = new Thread(() =>
        {
            SynchronizationContext.SetSynchronizationContext(
                new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher)
            );

            _window = new SplashScreenWindow();

            @event.Set();

            _window.Closed += (s, e) =>
                Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

            _window.Show();

            Dispatcher.Run();
        });

        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();

        @event.Wait();
    }

    public void SetProgress(double progress)
    {
        _window?.Dispatcher.BeginInvoke(() => _window.SetProgress(progress));
    }

    public void Dispose()
    {
        _window?.Dispatcher.BeginInvoke(() => _window?.Close());

        _thread.Join();
    }
}
