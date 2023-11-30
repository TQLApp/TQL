﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
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
    private Window _window;
    private readonly ManualResetEventSlim _windowCreatedEvent = new();
    private readonly ManualResetEventSlim _responseReceivedEvent = new();
    private readonly uint _windowMessage;
    private readonly uint _killWindowMessage;
    private readonly uint _responseWindowMessage;

    public bool IsFirstRunner { get; }

    public event EventHandler? Received;

    public WindowMessageIPC(string? environment, bool kill, ILogger logger)
    {
        _logger = logger;
        _windowMessage = RegisterWindowMessage(GetFullName(MessageName));
        _killWindowMessage = RegisterWindowMessage(GetFullName(KillMessageName));
        _responseWindowMessage = RegisterWindowMessage(GetFullName(ResponseMessageName));

        // Create a separate thread for the IPC window. This is to ensure it can
        // pump messages while the main thread is working, or, doing a synchronous
        // wait for an event like we do below.

        var thread = new Thread(ThreadProc) { IsBackground = true };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        _windowCreatedEvent.Wait();

        for (var i = 0; i < 5; i++)
        {
            logger.LogInformation($"[IPC] Attempt {i}");

            // If there is no window with the right name, we become the
            // first runner.

            var handle = FindWindow(null, GetFullName(WindowName));

            if (handle == IntPtr.Zero)
            {
                logger.LogInformation("[IPC] No other window found, we're the first runner");

                // Set the window caption to the right window name to
                // indicate we're the first runner.

                _window!.SetCaption(GetFullName(WindowName));

                IsFirstRunner = true;
                return;
            }

            // Send a message to the other window and wait for a response.

            logger.LogDebug("[IPC] Sending message to current first runner");

            PostMessage(
                handle,
                kill ? _killWindowMessage : _windowMessage,
                IntPtr.Zero,
                _window!.Handle
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

        _window = new Window(
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

        _windowCreatedEvent.Set();

        _logger.LogInformation(
            $"[IPC] Registering IPC window with handle {_window.Handle.ToInt64():x}"
        );

        Application.Run();

        _logger.LogInformation("[IPC] Background thread shutdown");

        void SendResponse(IntPtr handle)
        {
            PostMessage(handle, _responseWindowMessage, IntPtr.Zero, IntPtr.Zero);
        }
    }

    protected virtual void OnReceived() => Received?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _window.Dispose();
        _responseReceivedEvent.Dispose();
    }

    private sealed class Window : NativeWindow, IDisposable
    {
        private readonly WindowMessageIPC _owner;
        private readonly Action<IntPtr> _messageReceived;
        private readonly Action _responseMessageReceived;
        private readonly Action<IntPtr> _killMessageReceived;

        public Window(
            WindowMessageIPC owner,
            Action<IntPtr> messageReceived,
            Action responseMessageReceived,
            Action<IntPtr> killMessageReceived
        )
        {
            _owner = owner;
            _messageReceived = messageReceived;
            _responseMessageReceived = responseMessageReceived;
            _killMessageReceived = killMessageReceived;

            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m)
        {
            _owner._logger.LogInformation($"[IPC] Received window message {m.Msg}");

            base.WndProc(ref m);

            if (m.Msg == _owner._windowMessage)
                _messageReceived(m.LParam);
            if (m.Msg == _owner._responseWindowMessage)
                _responseMessageReceived();
            if (m.Msg == _owner._killWindowMessage)
                _killMessageReceived(m.LParam);
        }

        public void SetCaption(string caption)
        {
            SetWindowText(Handle, caption);
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern uint RegisterWindowMessage(string lpString);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);
}