using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Launcher.Abstractions;
using Microsoft.Extensions.Logging;
using Application = System.Windows.Forms.Application;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace Launcher.App.Services;

internal class UI : IUI, IDisposable
{
    private readonly ILogger<UI> _logger;
    private readonly Thread _thread;
    private readonly BlockingCollection<Action<IWin32Window>> _queue = new();
    private bool _disposed;

    public UI(ILogger<UI> logger)
    {
        _logger = logger;

        _thread = new Thread(ThreadProc);
        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();
    }

    private void ThreadProc()
    {
        _logger.LogInformation("Starting authentication thread");

        try
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using var form = new Form();

            _ = form.Handle;

            foreach (var item in _queue.GetConsumingEnumerable())
            {
                item(form);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure in authentication thread");
        }

        _logger.LogInformation("Shut down authentication thread");
    }

    public Task RunOnAuthenticationThread(Action<IWin32Window> func)
    {
        return RunOnAuthenticationThread(p =>
        {
            func(p);
            return true;
        });
    }

    public Task<T> RunOnAuthenticationThread<T>(Func<IWin32Window, T> func)
    {
        var tcs = new TaskCompletionSource<T>();

        _queue.Add(owner =>
        {
            try
            {
                tcs.SetResult(func(owner));
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task;
    }

    public void LaunchUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to launch '{Url}'", url);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _queue.CompleteAdding();
            _thread.Join();
            _queue.Dispose();

            _disposed = true;
        }
    }
}
