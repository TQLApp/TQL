using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Windows.Win32;
using Windows.Win32.Foundation;
using Application = System.Windows.Forms.Application;

namespace Tql.App.Services;

internal class WindowMessageIPC : IDisposable
{
    private readonly ILogger _logger;
    private const string MessageName = "$TQL.IPC^WM$";
    private const string KillMessageName = "$TQL.KILL.IPC^WM$";
    private const string ResponseMessageName = "$TQL.RESP.IPC^WM$";
    private const string WindowName = "$TQL.IPC$";

    private readonly SynchronizationContext _synchronizationContext =
        SynchronizationContext.Current ?? throw new InvalidOperationException();
    private IPCForm? _form;
    private IntPtr _handle;
    private readonly ManualResetEventSlim _windowCreatedEvent = new();
    private readonly ManualResetEventSlim _responseReceivedEvent = new();
    private readonly uint _windowMessage;
    private readonly uint _killWindowMessage;
    private readonly uint _responseWindowMessage;
    private readonly Thread _thread;

    public bool IsFirstRunner { get; }

    public event EventHandler? Received;

    public WindowMessageIPC(string? environment, bool kill, ILogger logger)
    {
        _logger = logger;
        _windowMessage = PInvoke.RegisterWindowMessage(GetFullName(MessageName));
        _killWindowMessage = PInvoke.RegisterWindowMessage(GetFullName(KillMessageName));
        _responseWindowMessage = PInvoke.RegisterWindowMessage(GetFullName(ResponseMessageName));

        // Create a separate thread for the IPC window. This is to ensure it can
        // pump messages while the main thread is working, or, doing a synchronous
        // wait for an event like we do below.

        _thread = new Thread(ThreadProc) { IsBackground = true };

        _thread.SetApartmentState(ApartmentState.STA);
        _thread.Start();

        _windowCreatedEvent.Wait();

        for (var i = 0; i < 5; i++)
        {
            logger.LogInformation($"[IPC] Attempt {i}");

            // If there is no window with the right name, we become the
            // first runner.

            var handle = PInvoke.FindWindow(null, GetFullName(WindowName));

            if (handle == IntPtr.Zero)
            {
                logger.LogInformation("[IPC] No other window found, we're the first runner");

                // Set the window caption to the right window name to
                // indicate we're the first runner.

                _form!.BeginInvoke(() => _form!.Text = GetFullName(WindowName));

                IsFirstRunner = true;
                return;
            }

            // Send a message to the other window and wait for a response.

            logger.LogDebug("[IPC] Sending message to current first runner");

            PInvoke.PostMessage(
                handle,
                kill ? _killWindowMessage : _windowMessage,
                default,
                _handle
            );

            var raised = _responseReceivedEvent.Wait(TimeSpan.FromSeconds(1));
            if (raised)
            {
                if (kill)
                    logger.LogInformation("[IPC] Response received, first runner is shutting down");
                else
                    logger.LogInformation("[IPC] Response received, we're not the first runner");

                // We received a response from the first runner so we
                // know that we're not the owner.
                return;
            }
        }

        logger.LogError("[IPC] Could not negotiate ownership");

        throw new WindowMessageIPCException("Could not negotiate ownership");

        string GetFullName(string name) =>
            !string.IsNullOrEmpty(environment) ? $"{name}#{environment!.ToUpperInvariant()}" : name;
    }

    private void ThreadProc()
    {
        // This window doesn't have the right name yet. It's here just to
        // receive the response. We only set the name if we become the owner.

        _form = new IPCForm(
            this,
            p =>
            {
                SendResponse(p);

                _synchronizationContext.Post(_ => OnReceived(), null);
            },
            () => _responseReceivedEvent.Set(),
            p =>
            {
                SendResponse(p);

                _logger.LogInformation("[IPC] Immediate shutdown requested");

                Environment.Exit(0);
            }
        );

        _handle = _form.Handle;

        _form.FormClosed += (_, _) => Application.ExitThread();

        _windowCreatedEvent.Set();

        _logger.LogInformation(
            $"[IPC] Registering IPC window with handle {_form.Handle.ToInt64():x}"
        );

        Application.Run();

        _logger.LogInformation("[IPC] Background thread shutdown");

        void SendResponse(IntPtr handle)
        {
            PInvoke.PostMessage(new HWND(handle), _responseWindowMessage, default, default);
        }
    }

    protected virtual void OnReceived() => Received?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _form?.BeginInvoke(() => _form.Close());
        _thread.Join();
        _responseReceivedEvent.Dispose();
    }

    private sealed class IPCForm(
        WindowMessageIPC owner,
        Action<IntPtr> messageReceived,
        Action responseMessageReceived,
        Action<IntPtr> killMessageReceived
    ) : Form
    {
        protected override void WndProc(ref Message m)
        {
            owner._logger.LogInformation($"[IPC] Received window message {m.Msg}");

            base.WndProc(ref m);

            if (m.Msg == owner._windowMessage)
                messageReceived(m.LParam);
            if (m.Msg == owner._responseWindowMessage)
                responseMessageReceived();
            if (m.Msg == owner._killWindowMessage)
                killMessageReceived(m.LParam);
        }

        protected override void SetVisibleCore(bool value)
        {
            base.SetVisibleCore(false);
        }
    }
}
